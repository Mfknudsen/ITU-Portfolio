using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Runtime.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Runtime.AI.EntitySystems
{
    [UpdateInGroup(typeof(NavigationSystemGroup))]
    [UpdateAfter(typeof(AgentAStarSystem))]
    [BurstCompile]
    internal partial struct AgentFunnelSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NavigationMeshSingletonComponent>();
            state.RequireForUpdate<UnitAgentComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity navmeshEntity = SystemAPI.GetSingletonEntity<NavigationMeshSingletonComponent>();

            DynamicBuffer<VertBufferElement> simpleVerts =
                SystemAPI.GetBuffer<VertBufferElement>(navmeshEntity);
            DynamicBuffer<NavTriangleBufferElement> triangles =
                SystemAPI.GetBuffer<NavTriangleBufferElement>(navmeshEntity);

            NativeList<Entity> entities = state.GetEntityQuery(ComponentType.ReadOnly<UnitAgentComponent>())
                .ToEntityListAsync(Allocator.TempJob, state.Dependency, out JobHandle handle);
            handle.Complete();
            AgentFunnelJob agentFunnelJob = new AgentFunnelJob(
                entities,
                triangles,
                simpleVerts,
                state.GetComponentLookup<DestinationComponent>(),
                state.GetComponentLookup<AgentSettingsComponent>(true),
                state.GetComponentLookup<LocalTransform>(true),
                state.GetBufferLookup<WayPointBufferElement>(),
                state.GetBufferLookup<AgentTrianglePathBufferElement>(true)
            );

            state.Dependency =
                agentFunnelJob.Schedule(entities, entities.Length / SystemInfo.processorCount, state.Dependency);
            state.CompleteDependency();
            entities.Dispose();
        }
    }

    [BurstCompile]
    internal struct AgentFunnelJob : IJobParallelForDefer
    {
        [DeallocateOnJobCompletion] [ReadOnly] private readonly NativeArray<NavTriangleBufferElement> triangles;
        [DeallocateOnJobCompletion] [ReadOnly] private readonly NativeArray<VertBufferElement> verts;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private NativeList<Entity> entities;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private ComponentLookup<AgentSettingsComponent> settingsLookup;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private ComponentLookup<LocalTransform> transformLookup;

        [NativeDisableParallelForRestriction] private ComponentLookup<DestinationComponent> destinationLookup;
        [NativeDisableParallelForRestriction] private BufferLookup<WayPointBufferElement> funnelPathLookup;
        [NativeDisableParallelForRestriction] private BufferLookup<AgentTrianglePathBufferElement> trianglePathLookup;

        public AgentFunnelJob(
            NativeList<Entity> entities,
            DynamicBuffer<NavTriangleBufferElement> triangles,
            DynamicBuffer<VertBufferElement> simpleVerts,
            ComponentLookup<DestinationComponent> destinationLookup,
            ComponentLookup<AgentSettingsComponent> settingsLookup,
            ComponentLookup<LocalTransform> transformLookup,
            BufferLookup<WayPointBufferElement> funnelPathLookup,
            BufferLookup<AgentTrianglePathBufferElement> trianglePathLookup) : this()
        {
            this.entities = entities;
            this.triangles = triangles.ToNativeArray(Allocator.TempJob);
            this.verts = simpleVerts.ToNativeArray(Allocator.TempJob);

            this.destinationLookup = destinationLookup;
            this.settingsLookup = settingsLookup;
            this.transformLookup = transformLookup;
            this.funnelPathLookup = funnelPathLookup;
            this.trianglePathLookup = trianglePathLookup;
        }

        [BurstCompile]
        public void Execute(int index)
        {
            Entity entity = this.entities[index];
            DestinationComponent destination = this.destinationLookup[entity];
            AgentSettingsComponent agentSettings = this.settingsLookup[entity];
            LocalTransform transform = this.transformLookup[entity];
            DynamicBuffer<WayPointBufferElement> agentFunnelPath = this.funnelPathLookup[entity];
            DynamicBuffer<AgentTrianglePathBufferElement> agentTrianglePath = this.trianglePathLookup[entity];

            int pathIndex = 0;
            //Result list containing the different position for the agent to travel along.
            if (destination.TrianglePathCount > 1)
            {
                //Apex is the newest point the agent will travel to.
                float3 apex = transform.Position;
                //List of portals to check.
                this.GetPortals(apex,
                    agentTrianglePath, destination.TrianglePathCount,
                    agentSettings.Radius,
                    out NativeHashMap<int, float3> remappedVerts,
                    out NativeArray<int> rightArray,
                    out NativeArray<int> leftArray);
                int portalCount = rightArray.Length;

                if (destination.Debug)
                {
                    for (int i = 1; i < destination.TrianglePathCount; i++)
                    {
                        Debug.DrawLine(this.triangles[agentTrianglePath[i].Index].Center,
                            this.triangles[agentTrianglePath[i - 1].Index].Center, Color.yellow);
                    }

                    for (int i = 1; i < portalCount; i++)
                    {
                        Debug.DrawLine(remappedVerts[rightArray[i]] + new float3(0, 3, 0),
                            remappedVerts[rightArray[i - 1]] + new float3(0, 3, 0), Color.red);
                        Debug.DrawLine(remappedVerts[leftArray[i]] + new float3(0, 3, 0),
                            remappedVerts[leftArray[i - 1]] + new float3(0, 3, 0), Color.blue);
                    }
                }

                //Portal vert ids is the current funnel.
                float3 portalLeft = remappedVerts[leftArray[0]],
                    portalRight = remappedVerts[rightArray[0]];

                //Ids to be used when setting the new apex and adding to the result.
                int leftID = leftArray[0],
                    rightID = rightArray[0],
                    //Used when resetting the for loop to the portal which the newest apex originates from.
                    leftPortalID = 0,
                    rightPortalID = 0;

                //Checking the portals.
                for (int i = 1; i < leftArray.Length; i++)
                {
                    //The newest points to be checked against the current funnel.
                    float3 left = remappedVerts[leftArray[i]],
                        right = remappedVerts[rightArray[i]];

                    if (TriArea2(apex, portalRight, right) <= 0f)
                    {
                        //Update right if the new right point is within the funnel or the apex is the current right point
                        if (apex.Equals(portalRight) ||
                            TriArea2(apex, portalLeft, right) > 0f)
                        {
                            //Tighten the funnel
                            portalRight = right;
                            rightPortalID = i;

                            if (i < portalCount)
                                rightID = rightArray[i];
                        }
                        else
                        {
                            //Right over left, insert left to path and restart scan from portal left point
                            AddToPath(ref agentFunnelPath, ref pathIndex, remappedVerts[leftID], true);

                            //Make current left the new apex
                            apex = portalLeft;
                            portalRight = portalLeft;

                            //Reset
                            i = leftPortalID;
                            continue;
                        }
                    }

                    if (!(TriArea2(apex, portalLeft, left) >= 0f)) continue;

                    //Update left if the new left point is within the funnel or the apex is the current left point
                    if (apex.Equals(portalLeft) ||
                        TriArea2(apex, portalRight, left) < 0f)
                    {
                        //Tighten the funnel
                        portalLeft = left;
                        leftPortalID = i;

                        if (i < portalCount)
                            leftID = leftArray[i];
                    }
                    else
                    {
                        //Left over right, insert right to path and restart scan from portal right point
                        AddToPath(ref agentFunnelPath, ref pathIndex, remappedVerts[rightID], true);

                        apex = portalRight;
                        portalLeft = portalRight;
                        leftPortalID = rightPortalID;

                        //Reset
                        i = rightPortalID;
                    }
                }

                //The end point might be outside the current funnel after the original algorithm.
                //This catches those. 
                float3 e = destination.Point;
                bool apexFound = true;
                while (apexFound)
                {
                    apexFound = false;
                    if (TriArea2(apex, portalRight, e) > 0)
                    {
                        AddToPath(ref agentFunnelPath, ref pathIndex, remappedVerts[rightID], true);

                        apex = portalRight;
                        portalLeft = apex;
                        if (rightPortalID + 1 < portalCount)
                        {
                            rightPortalID++;
                            rightID = rightArray[rightPortalID];
                            portalRight = remappedVerts[rightID];
                        }

                        apexFound = true;
                        continue;
                    }

                    if (!(TriArea2(apex, portalLeft, e) < 0)) continue;

                    AddToPath(ref agentFunnelPath, ref pathIndex, remappedVerts[leftID], true);

                    apex = portalLeft;
                    portalRight = apex;
                    if (leftPortalID + 1 < portalCount)
                    {
                        leftPortalID++;
                        leftID = leftArray[leftPortalID];
                        portalLeft = remappedVerts[leftID];
                    }

                    apexFound = true;
                }
            }

            AddToPath(ref agentFunnelPath, ref pathIndex, destination.Point, true);
            destination.FunnelPathCount = pathIndex;

            if (!destination.Debug)
                return;

            Debug.DrawLine(transform.Position + new float3(0, 1, 0),
                agentFunnelPath[0].Point + new float3(0, 1, 0), Color.green);

            for (int i = 1; i < pathIndex; i++)
            {
                Debug.DrawLine(agentFunnelPath[i].Point + new float3(0, 1, 0),
                    agentFunnelPath[i - 1].Point + new float3(0, 1, 0), Color.green);
            }
        }

        [BurstCompile]
        private static void AddToPath(ref DynamicBuffer<WayPointBufferElement> path, ref int index, in float3 point,
            in bool isWalk)
        {
            if (index < path.Length)
            {
                WayPointBufferElement p = path[index];
                p.Point = point;
                p.IsWalk = isWalk;
                path[index] = p;
            }
            else
                path.Add(new WayPointBufferElement { IsWalk = isWalk, Point = point });

            index++;
        }

        [BurstCompile]
        private void GetPortals(in float3 start,
            in DynamicBuffer<AgentTrianglePathBufferElement> trianglePathIDs, int triangleIdsSize,
            float agentRadius,
            out NativeHashMap<int, float3> remappedVerts,
            out NativeArray<int> rightArray, out NativeArray<int> leftArray)
        {
            NativeList<int> rightArrayResult = new NativeList<int>(triangleIdsSize, Allocator.Temp),
                leftArrayResult = new NativeList<int>(triangleIdsSize, Allocator.Temp);

            NativeHashMap<int, float3> remapped =
                new NativeHashMap<int, float3>((triangleIdsSize - 1) * 2, Allocator.Temp);

            NativeArray<int> reuseArray = new NativeArray<int>(3, Allocator.Temp);

            int vertAIndex, vertBIndex;
            for (int i = 0; i < triangleIdsSize - 1; i++)
            {
                NavTriangleBufferElement current = this.triangles[trianglePathIDs[i].Index];

                int nextIndex = trianglePathIDs[i + 1].Index;

                if (current.NeighborOneId == nextIndex)
                {
                    vertAIndex = current.NeighborOneA;
                    vertBIndex = current.NeighborOneB;
                }
                else if (current.NeighborTwoId == nextIndex)
                {
                    vertAIndex = current.NeighborTwoA;
                    vertBIndex = current.NeighborTwoB;
                }
                else
                {
                    vertAIndex = current.NeighborThreeA;
                    vertBIndex = current.NeighborThreeB;
                }

                current.Vertices(ref reuseArray);

                float3 ab = this.verts[vertAIndex].Position -
                            this.verts[vertBIndex].Position;

                if (remapped.ContainsKey(vertAIndex))
                    remapped[vertAIndex] -= ab;
                else
                    remapped.Add(vertAIndex, -ab);

                if (remapped.ContainsKey(vertBIndex))
                    remapped[vertBIndex] += ab;
                else
                    remapped.Add(vertBIndex, ab);
            }

            foreach (KVPair<int, float3> kvPair in remapped)
            {
                if (kvPair.Value.QuickSquareDistance() == 0)
                {
                    kvPair.Value = this.verts[kvPair.Key].Position;
                    continue;
                }

                kvPair.Value = this.verts[kvPair.Key].Position + kvPair.Value.Normalize() * agentRadius;
            }

            // for (int i = 0; i < oldVertIndex.Length; i++)
            // {
            //     float3 vert = this.verts[oldVertIndex[i]].Position * (agentRadius * 1.25f);
            //
            //     remappedVertsResult[i] = vert;
            // }

            remappedVerts = remapped;
            NativeArray<int> reuseSharedArray = new NativeArray<int>(2, Allocator.Temp);

            //Creating portals
            Shared(this.triangles[trianglePathIDs[0].Index], this.triangles[trianglePathIDs[1].Index],
                ref reuseSharedArray);
            vertAIndex = reuseSharedArray[0];
            vertBIndex = reuseSharedArray[1];
            float3 forwardEnd = remapped[vertAIndex] +
                                (remappedVerts[vertBIndex] -
                                 remappedVerts[vertAIndex]) * .5f;

            bool left = MathC.IsPointLeftToVector(start, forwardEnd, remappedVerts[vertAIndex]);

            if (left)
            {
                leftArrayResult.Add(vertAIndex);
                rightArrayResult.Add(vertBIndex);
            }
            else
            {
                leftArrayResult.Add(vertBIndex);
                rightArrayResult.Add(vertAIndex);
            }

            for (int i = 0; i < triangleIdsSize - 1; i++)
            {
                NavTriangleBufferElement current = this.triangles[trianglePathIDs[i].Index];

                int nextIndex = trianglePathIDs[i + 1].Index;

                if (current.NeighborOneId == nextIndex)
                {
                    vertAIndex = current.NeighborOneA;
                    vertBIndex = current.NeighborOneB;
                }
                else if (current.NeighborTwoId == nextIndex)
                {
                    vertAIndex = current.NeighborTwoA;
                    vertBIndex = current.NeighborTwoB;
                }
                else
                {
                    vertAIndex = current.NeighborThreeA;
                    vertBIndex = current.NeighborThreeB;
                }

                if (leftArrayResult[^1] == vertAIndex || rightArrayResult[^1] == vertBIndex)
                {
                    leftArrayResult.Add(vertAIndex);
                    rightArrayResult.Add(vertBIndex);
                }
                else
                {
                    leftArrayResult.Add(vertBIndex);
                    rightArrayResult.Add(vertAIndex);
                }
            }

            rightArray = rightArrayResult;
            leftArray = leftArrayResult;
        }

        /// <summary>
        ///     Calculates if clockwise or counter clockwise
        /// </summary>
        /// <param name="a">Apex</param>
        /// <param name="b">Portal point</param>
        /// <param name="c">New point</param>
        /// <returns>Returns positive value if clockwise and negative value if counter clockwise</returns>
        [BurstCompile]
        private static float TriArea2(in float3 a, in float3 b, in float3 c)
        {
            float ax = b.x - a.x;
            float ay = b.z - a.z;
            float bx = c.x - a.x;
            float by = c.z - a.z;
            return bx * ay - ax * by;
        }

        [BurstCompile]
        private static int Contains(in NativeList<int> array, in int index)
        {
            for (int i = array.Length - 1; i >= 0; i--)
            {
                if (array[i] == index)
                    return i;
            }

            return -1;
        }

        [BurstCompile]
        private static void Shared(in NavTriangleBufferElement a, in NavTriangleBufferElement b,
            ref NativeArray<int> result)
        {
            int i = 0;
            if (a.A == b.A)
            {
                result[i] = a.A;
                i++;
            }

            if (a.A == b.B)
            {
                result[i] = a.A;
                i++;
            }

            if (a.A == b.C)
            {
                result[i] = a.A;
                i++;
            }

            if (a.B == b.A)
            {
                result[i] = a.B;
                i++;
            }

            if (a.B == b.B)
            {
                result[i] = a.B;
                i++;
            }

            if (a.B == b.C)
            {
                result[i] = a.B;
                i++;
            }

            if (a.C == b.A)
            {
                result[i] = a.C;
                i++;
            }

            if (a.C == b.B)
            {
                result[i] = a.C;
                i++;
            }

            if (a.C == b.C)
            {
                result[i] = a.C;
            }
        }
    }
}