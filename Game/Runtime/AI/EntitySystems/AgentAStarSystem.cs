using System;
using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Runtime.Core;
using Runtime.Variables.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.AI.EntitySystems
{
    [UpdateInGroup(typeof(NavigationSystemGroup))]
    [BurstCompile]
    public partial struct AgentAStarSystem : ISystem
    {
        private EntityQuery entityQuery;

        private BufferLookup<AgentTrianglePathBufferElement> agentPathLookup;
        private ComponentLookup<DestinationComponent> destinationLookup;
        private ComponentLookup<UnitAgentComponent> agentLookup;
        private ComponentLookup<AgentSettingsComponent> agentSettingsLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NavigationMeshSingletonComponent>();
            state.RequireForUpdate<UnitAgentComponent>();

            this.entityQuery = state.GetEntityQuery(ComponentType.ReadOnly<UnitAgentComponent>(),
                ComponentType.ReadWrite<DestinationComponent>());

            this.agentPathLookup = state.GetBufferLookup<AgentTrianglePathBufferElement>();
            this.destinationLookup = state.GetComponentLookup<DestinationComponent>();
            this.agentLookup = state.GetComponentLookup<UnitAgentComponent>(true);
            this.agentSettingsLookup = state.GetComponentLookup<AgentSettingsComponent>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonEntity<NavigationMeshSingletonComponent>(out Entity navmeshEntity))
                return;

            NativeArray<Entity> entities = this.entityQuery
                .ToEntityArray(Allocator.TempJob);

            if (entities.Length == 0)
            {
                state.Dependency = entities.Dispose(state.Dependency);
                return;
            }

            NativeArray<NavTriangleBufferElement> triangles =
                SystemAPI.GetBuffer<NavTriangleBufferElement>(navmeshEntity).ToNativeArray(Allocator.TempJob);
            NativeArray<AreaBufferElement> areas = SystemAPI.GetBuffer<AreaBufferElement>(navmeshEntity)
                .ToNativeArray(Allocator.TempJob);

            this.agentPathLookup.Update(ref state);
            this.destinationLookup.Update(ref state);
            this.agentLookup.Update(ref state);
            this.agentSettingsLookup.Update(ref state);

            int batch = math.max(entities.Length / SystemInfo.processorCount, 1);

            AgentAStarJob pathingJob = new AgentAStarJob(
                in entities,
                in triangles,
                in areas,
                this.agentPathLookup,
                this.destinationLookup,
                this.agentLookup,
                this.agentSettingsLookup
            );

            JobHandle handle = pathingJob.ScheduleParallel(entities.Length, batch, state.Dependency);

            JobHandle entitiesDispose = entities.Dispose(handle);
            JobHandle trianglesDispose = triangles.Dispose(entitiesDispose);
            JobHandle areasDispose = areas.Dispose(trianglesDispose);

            state.Dependency = areasDispose;
        }
    }

    [BurstCompile]
    internal struct AgentAStarJob : IJobFor
    {
        [ReadOnly] private NativeArray<AreaBufferElement> areaTypes;
        [ReadOnly] private NativeArray<NavTriangleBufferElement> triangles;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private NativeArray<Entity> entities;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private ComponentLookup<AgentSettingsComponent> agentSettingsLookup;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private ComponentLookup<UnitAgentComponent> agentLookup;

        [NativeDisableParallelForRestriction] private BufferLookup<AgentTrianglePathBufferElement> agentPathLookup;
        [NativeDisableParallelForRestriction] private ComponentLookup<DestinationComponent> destinationLookup;

        public AgentAStarJob(
            in NativeArray<Entity> entities,
            in NativeArray<NavTriangleBufferElement> triangles,
            in NativeArray<AreaBufferElement> areaType,
            in BufferLookup<AgentTrianglePathBufferElement> agentPathLookup,
            in ComponentLookup<DestinationComponent> destinationLookup,
            in ComponentLookup<UnitAgentComponent> agentLookup,
            in ComponentLookup<AgentSettingsComponent> agentSettingsLookup) : this()
        {
            this.entities = entities;
            this.triangles = triangles;
            this.areaTypes = areaType;

            this.agentPathLookup = agentPathLookup;
            this.destinationLookup = destinationLookup;
            this.agentLookup = agentLookup;
            this.agentSettingsLookup = agentSettingsLookup;
        }

        [BurstCompile]
        public void Execute(int index)
        {
            Entity entity = this.entities[index];
            UnitAgentComponent agent = this.agentLookup[entity];

            if (agent.CurrentTriangleID == -1)
                return;

            DestinationComponent destination = this.destinationLookup[entity];
            if (destination.TriangleID == -1)
                return;

            if (!destination.Refresh)
                return;

            DynamicBuffer<AgentTrianglePathBufferElement> agentPath = this.agentPathLookup[entity];
            AgentSettingsComponent agentSettings = this.agentSettingsLookup[entity];

            NativePriorityHeap<JobNodeEcs> toCheckNodes =
                new NativePriorityHeap<JobNodeEcs>(256, Allocator.Temp);

            NativeHashMap<int, JobNodeEcs> results = new NativeHashMap<int, JobNodeEcs>(256, Allocator.Temp);

            JobNodeEcs checking = new JobNodeEcs(this.triangles[agent.CurrentTriangleID],
                destination.Point);
            toCheckNodes.Push(checking);
            results.Add(checking.ID, checking);

#if UNITY_EDITOR && !UNITY_DOTSPLAYER
            if (destination.Debug)
            {
                Debug.DrawRay(this.triangles[agent.CurrentTriangleID].Center, Vector3.up, Color.green);
                Debug.DrawRay(this.triangles[destination.TriangleID].Center, Vector3.up, Color.green);
            }
#endif

            while (toCheckNodes.Count > 0 && checking.ID != destination.TriangleID)
            {
                checking = toCheckNodes.Pop();

                NavTriangleBufferElement triangle = this.triangles[checking.ID];

                if (checking.ID == destination.TriangleID)
                    break;

                for (int neighborIndex = 0; neighborIndex < 3; neighborIndex++)
                {
                    int neighborTriangleId = neighborIndex switch
                    {
                        0 => triangle.NeighborOneId,
                        1 => triangle.NeighborTwoId,
                        2 => triangle.NeighborThreeId,
                        _ => -1
                    };

                    if (neighborTriangleId == -1)
                        break;

                    if (neighborIndex switch
                        {
                            0 => triangle.NeighborOneWidth2D,
                            1 => triangle.NeighborTwoWidth2D,
                            2 => triangle.NeighborThreeWidth2D,
                            _ => 0
                        } < agentSettings.Radius * 2f)
                        continue;


                    if (results.ContainsKey(neighborTriangleId))
                        continue;

#if UNITY_EDITOR && !UNITY_DOTSPLAYER
                    if (destination.Debug)
                        Debug.DrawLine(triangle.Center, this.triangles[neighborTriangleId].Center, Color.yellow);
#endif

                    JobNodeEcs j = new JobNodeEcs(
                        this.triangles[neighborTriangleId],
                        destination.Point,
                        results[checking.ID],
                        triangle.Center,
                        this.areaTypes);

                    results.Add(j.ID, j);

                    toCheckNodes.Push(j);
                }
            }

#if UNITY_EDITOR && !UNITY_DOTSPLAYER
            if (checking.ID != destination.TriangleID)
            {
                Debug.Log($"E: {toCheckNodes.Count}");
            }
#endif

            //Retrace backwards towards current triangle
            int pathIndex = 0;

            while (checking.ParentID != -1)
            {
                AddToPath(ref agentPath, ref pathIndex, checking.ID);

                checking = results[checking.ParentID];
            }

            AddToPath(ref agentPath, ref pathIndex, checking.ID);

            destination.TrianglePathCount = pathIndex;

            //Reverse the path
            int half = Mathf.FloorToInt(pathIndex * 0.5f);
            for (int i = 0; i < half; i++)
                (agentPath[i], agentPath[pathIndex - 1 - i]) = (agentPath[pathIndex - 1 - i], agentPath[i]);

            toCheckNodes.Dispose();
            results.Dispose();

            this.destinationLookup[entity] = destination;
        }

        private static void AddToPath(ref DynamicBuffer<AgentTrianglePathBufferElement> agentPath, ref int index,
            int id)
        {
            if (agentPath.Length > index)
            {
                AgentTrianglePathBufferElement t = agentPath[index];
                t.Index = id;
                agentPath[index] = t;
            }
            else
                agentPath.Add(new AgentTrianglePathBufferElement(id));

            index++;
        }
    }

    [BurstCompile]
    internal readonly struct JobNodeEcs : IComparable<JobNodeEcs>
    {
        public readonly int ParentID, ID;
        private readonly float moveCost, distanceToGoal;

        public JobNodeEcs(
            in NavTriangleBufferElement currentTriangle,
            in float3 destination,
            in JobNodeEcs previousJobNode,
            in float3 previousTriangleCenter,
            in NativeArray<AreaBufferElement> areas)
        {
            this.ParentID = previousJobNode.ID;
            this.ID = currentTriangle.ID;

            this.moveCost = previousJobNode.MoveCost() +
                            currentTriangle.Center.QuickSquareDistance(previousTriangleCenter);
            this.distanceToGoal = currentTriangle.Center.QuickSquareDistance(destination);
        }

        public JobNodeEcs(in NavTriangleBufferElement currentTriangle,
            in float3 destination)
        {
            this.ParentID = -1;
            this.ID = currentTriangle.ID;
            this.moveCost = 0;

            this.distanceToGoal = currentTriangle.Center.QuickSquareDistance(destination);
        }

        [BurstCompile]
        private float Total()
        {
            return this.moveCost + this.distanceToGoal;
        }

        [BurstCompile]
        private float MoveCost()
        {
            return this.moveCost;
        }

        [BurstCompile]
        public int CompareTo(JobNodeEcs other)
        {
            return this.Total().CompareTo(other.Total());
        }
    }
}