using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Runtime.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Runtime.AI.EntitySystems
{
    [BurstCompile]
    public partial struct AgentFunnelJob : IJobEntity
    {
        [DeallocateOnJobCompletion] [ReadOnly] private readonly NativeArray<NavTriangleBufferElement> triangles;
        [DeallocateOnJobCompletion] [ReadOnly] private readonly NativeArray<VertXZBufferElement> simpleVerts;
        [DeallocateOnJobCompletion] [ReadOnly] private readonly NativeArray<VertYBufferElement> vertsY;

        public AgentFunnelJob(DynamicBuffer<NavTriangleBufferElement> triangles,
            DynamicBuffer<VertXZBufferElement> simpleVerts, DynamicBuffer<VertYBufferElement> vertsY) : this()
        {
            this.triangles = triangles.ToNativeArray(Allocator.TempJob);
            this.simpleVerts = simpleVerts.ToNativeArray(Allocator.TempJob);
            this.vertsY = vertsY.ToNativeArray(Allocator.TempJob);
        }

        [BurstCompile]
        public void Execute(ref DynamicBuffer<WayPointBufferElement> agentFunnelPath,
            in DynamicBuffer<AgentTrianglePathBufferElement> agentTrianglePath, in AgentSettingsComponent agentSettings,
            in DestinationComponent destination, in LocalTransform transform)
        {
            int pathIndex = 0;
            //Result list containing the different position for the agent to travel along.
            if (agentTrianglePath.Length > 1)
            {
                //Apex is the newest point the agent will travel to.
                float2 apex = transform.Position.XZ();
                //List of portals to check.
                this.GetPortals(apex,
                    agentTrianglePath,
                    agentSettings.Radius,
                    out NativeArray<float2> remappedSimpleVerts,
                    out NativeArray<float3> remappedVerts,
                    out NativeList<int> rightArray,
                    out NativeList<int> leftArray);

                return;

                int portalCount = rightArray.Length;

                //Portal vert ids is the current funnel.
                float2 portalLeft = remappedSimpleVerts[leftArray[0]],
                    portalRight = remappedSimpleVerts[rightArray[0]];

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
                    float2 left = remappedSimpleVerts[leftArray[i]],
                        right = remappedSimpleVerts[rightArray[i]];

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

                            /*
                            agentFunnelPath.Add(new WayPointBufferElement()
                            {
                                Point = remappedVerts[leftID],
                                IsWalk = true
                            });
                            */
                            //Make current left the new apex
                            apex = portalLeft;
                            portalRight = portalLeft;

                            //Reset
                            i = leftPortalID;
                            continue;
                        }
                    }

                    if (TriArea2(apex, portalLeft, left) >= 0f)
                    {
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

                            /*
                            agentFunnelPath.Add(new WayPointBufferElement()
                            {
                                 Point = remappedVerts[rightID],
                                 IsWalk = true
                             });
                            */
                            //Make current right the new apex
                            apex = portalRight;
                            portalLeft = portalRight;
                            leftPortalID = rightPortalID;

                            //Reset
                            i = rightPortalID;
                        }
                    }
                }

                //The end point might be outside the current funnel after the original algorithm.
                //This catches those. 
                float2 e = destination.Point.XZ();
                bool apexFound = true;
                while (apexFound)
                {
                    apexFound = false;
                    if (TriArea2(apex, portalRight, e) > 0)
                    {
                        AddToPath(ref agentFunnelPath, ref pathIndex, remappedVerts[rightID], true);

                        /*
                        agentFunnelPath.Add(new WayPointBufferElement()
                        {
                            Point = remappedVerts[rightID],
                            IsWalk = true
                        });
                        */
                        apex = portalRight;
                        portalLeft = apex;
                        if (rightPortalID + 1 < portalCount)
                        {
                            rightPortalID++;
                            rightID = rightArray[rightPortalID];
                            portalRight = remappedSimpleVerts[rightID];
                        }

                        apexFound = true;
                        continue;
                    }

                    if (TriArea2(apex, portalLeft, e) < 0)
                    {
                        AddToPath(ref agentFunnelPath, ref pathIndex, remappedVerts[leftID], true);

                        /*
                        agentFunnelPath.Add(new WayPointBufferElement()
                        {
                            Point = remappedVerts[leftID],
                            IsWalk = true
                        });
                        */
                        apex = portalLeft;
                        portalRight = apex;
                        if (leftPortalID + 1 < portalCount)
                        {
                            leftPortalID++;
                            leftID = leftArray[leftPortalID];
                            portalLeft = remappedSimpleVerts[leftID];
                        }

                        apexFound = true;
                    }
                }
            }

            AddToPath(ref agentFunnelPath, ref pathIndex, destination.Point, true);
            /*
            agentFunnelPath.Add(new WayPointBufferElement()
            {
                Point = destination.Point,
                IsWalk = true
            });
            */
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
        private void GetPortals(in float2 start, in DynamicBuffer<AgentTrianglePathBufferElement> triangleIDs,
            float agentRadius,
            out NativeArray<float2> remappedSimpleVerts, out NativeArray<float3> remappedVerts,
            out NativeList<int> rightArray, out NativeList<int> leftArray)
        {
            //RemappingVertices
            NativeArray<float3> remappedVertsResult =
                new NativeArray<float3>((triangleIDs.Length - 1) * 2, Allocator.Temp);
            NativeArray<float2> remappedSimpleVertsResult =
                new NativeArray<float2>((triangleIDs.Length - 1) * 2, Allocator.Temp);

            NativeArray<int> shared = new NativeArray<int>(2, Allocator.Temp);

            NativeList<int> rightArrayResult = new NativeList<int>(this.vertsY.Length / 2, Allocator.Temp),
                leftArrayResult = new NativeList<int>(this.vertsY.Length / 2, Allocator.Temp);

            NativeList<int> oldVertIndex = new NativeList<int>((triangleIDs.Length - 1) * 2, Allocator.Temp);
            NativeList<float3> directions = new NativeList<float3>(oldVertIndex.Length, Allocator.Temp);

            for (int i = 1; i < triangleIDs.Length; i++)
            {
                Shared(this.triangles[triangleIDs[i].Index], this.triangles[triangleIDs[i - 1].Index],
                    ref shared);

                float3 ab = this.simpleVerts[shared[0]].ToV3(this.vertsY[shared[0]]) -
                            this.simpleVerts[shared[1]].ToV3(this.vertsY[shared[1]]);

                int index = Contains(oldVertIndex, shared[0]);
                if (index == -1)
                {
                    oldVertIndex.Add(shared[0]);
                    directions.Add(-ab);
                }
                else
                {
                    directions[index] -= ab;
                }

                index = Contains(oldVertIndex, shared[1]);
                if (index == -1)
                {
                    oldVertIndex.Add(shared[1]);
                    directions.Add(ab);
                }
                else
                {
                    directions[index] += ab;
                }
            }

            for (int i = 0; i < oldVertIndex.Length; i++)
            {
                float3 vert = this.simpleVerts[oldVertIndex[i]].ToV3(this.vertsY[oldVertIndex[i]]) +
                              directions[i].Normalize() * agentRadius * 1.25f;
                remappedVertsResult[i] = vert;
                remappedSimpleVertsResult[i] = vert.XZ();
            }

            remappedVerts = remappedVertsResult;
            remappedSimpleVerts = remappedSimpleVertsResult;

            //Creating portals
            Shared(this.triangles[triangleIDs[0].Index], this.triangles[triangleIDs[1].Index], ref shared);
            int a = Contains(oldVertIndex, shared[0]), b = Contains(oldVertIndex, shared[1]);
            float2 forwardEnd = remappedSimpleVerts[a] +
                                (remappedSimpleVerts[b] -
                                 remappedSimpleVerts[a]) * .5f;
            bool left = MathC.IsPointLeftToVector(start, forwardEnd, remappedSimpleVerts[0]);

            if (left)
            {
                leftArrayResult.Add(0);
                rightArrayResult.Add(1);
            }
            else
            {
                leftArrayResult.Add(1);
                rightArrayResult.Add(0);
            }

            for (int i = 1; i < triangleIDs.Length - 1; i++)
            {
                Shared(this.triangles[triangleIDs[i].Index], this.triangles[triangleIDs[i + 1].Index], ref shared);
                a = Contains(oldVertIndex, shared[0]);
                b = Contains(oldVertIndex, shared[1]);
                if (leftArrayResult[^1] == a || rightArrayResult[^1] == b)
                {
                    leftArrayResult.Add(a);
                    rightArrayResult.Add(b);
                }
                else
                {
                    leftArrayResult.Add(b);
                    rightArrayResult.Add(a);
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
        private static float TriArea2(in float2 a, in float2 b, in float2 c)
        {
            float ax = b.x - a.x;
            float ay = b.y - a.y;
            float bx = c.x - a.x;
            float by = c.y - a.y;
            return bx * ay - ax * by;
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
    }
}