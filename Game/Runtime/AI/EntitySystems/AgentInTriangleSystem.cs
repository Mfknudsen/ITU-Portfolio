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
    [BurstCompile]
    internal partial struct AgentInTriangleSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NavigationMeshSingletonComponent>();
            state.RequireForUpdate<NavigationMeshSingletonComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity navmeshEntity = SystemAPI.GetSingletonEntity<NavigationMeshSingletonComponent>();

            NavigationMeshSingletonComponent navmeshSingletonComponent =
                SystemAPI.GetSingleton<NavigationMeshSingletonComponent>();

            DynamicBuffer<TriangleFlattenBufferElement> sizeBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenBufferElement>(navmeshEntity);

            if (sizeBufferElements.Length == 0)
                return;

            DynamicBuffer<TriangleFlattenIndexBufferElement> triangleIndexBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenIndexBufferElement>(navmeshEntity);
            DynamicBuffer<VertBufferElement> verts =
                SystemAPI.GetBuffer<VertBufferElement>(navmeshEntity);
            DynamicBuffer<NavTriangleBufferElement> triangles =
                SystemAPI.GetBuffer<NavTriangleBufferElement>(navmeshEntity);

            NativeList<Entity> entities = state.GetEntityQuery(ComponentType.ReadOnly<UnitAgentComponent>())
                .ToEntityListAsync(Allocator.TempJob, state.Dependency, out JobHandle handle);
            handle.Complete();

            AgentInTriangleJob agentInTriangleJob = new AgentInTriangleJob(
                entities,
                navmeshSingletonComponent.GroupDivision,
                navmeshSingletonComponent.MinFloorX,
                navmeshSingletonComponent.MinFloorZ,
                navmeshSingletonComponent.CellXLength,
                navmeshSingletonComponent.CellZLength,
                verts,
                triangles,
                sizeBufferElements,
                triangleIndexBufferElements,
                state.GetComponentLookup<LocalTransform>(),
                state.GetComponentLookup<UnitAgentComponent>(),
                state.GetComponentLookup<AgentSettingsComponent>());
            state.Dependency = agentInTriangleJob.Schedule(entities, entities.Length / SystemInfo.processorCount,
                state.Dependency);
            state.CompleteDependency();
            entities.Dispose();
        }
    }

    [BurstCompile]
    internal struct AgentInTriangleJob : IJobParallelForDefer
    {
        [ReadOnly] private readonly float groupDivision;

        [ReadOnly] private readonly int cellXLength, cellZLength;
        [ReadOnly] private readonly float minFloorX, minFloorZ;

        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<VertBufferElement> verts;
        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<NavTriangleBufferElement> triangles;

        //Used to flatten a 3D array 
        [DeallocateOnJobCompletion] [ReadOnly]
        private readonly NativeArray<TriangleFlattenBufferElement> triangleArraySizes;

        [DeallocateOnJobCompletion] [ReadOnly]
        private readonly NativeArray<TriangleFlattenIndexBufferElement> triangleIndexes;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private NativeList<Entity> entities;

        [NativeDisableParallelForRestriction] private ComponentLookup<UnitAgentComponent> agentLookup;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private ComponentLookup<LocalTransform> transformLookup;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private ComponentLookup<AgentSettingsComponent> settingLookup;

        public AgentInTriangleJob(
            NativeList<Entity> entities,
            float groupDivision, float minFloorX, float minFloorZ, int cellXLength, int cellZLength,
            DynamicBuffer<VertBufferElement> verts,
            DynamicBuffer<NavTriangleBufferElement> triangles,
            DynamicBuffer<TriangleFlattenBufferElement> sizeArray,
            DynamicBuffer<TriangleFlattenIndexBufferElement> indexList,
            ComponentLookup<LocalTransform> transformLookup,
            ComponentLookup<UnitAgentComponent> agentLookup,
            ComponentLookup<AgentSettingsComponent> settingLookup) : this()
        {
            this.entities = entities;

            this.groupDivision = groupDivision;

            this.minFloorX = minFloorX;
            this.minFloorZ = minFloorZ;

            this.cellXLength = cellXLength;
            this.cellZLength = cellZLength;

            this.verts = verts.ToNativeArray(Allocator.TempJob);
            this.triangles = triangles.ToNativeArray(Allocator.TempJob);

            this.triangleArraySizes = sizeArray.ToNativeArray(Allocator.TempJob);
            this.triangleIndexes = indexList.ToNativeArray(Allocator.TempJob);

            this.transformLookup = transformLookup;
            this.agentLookup = agentLookup;
            this.settingLookup = settingLookup;
        }

        [BurstCompile]
        public void Execute(int index)
        {
            Entity entity = this.entities[index];
            LocalTransform transform = this.transformLookup[entity];
            UnitAgentComponent agent = this.agentLookup[entity];

            float3 agentPosition = transform.Position.xyz;
            float2 agentPosition2D = agentPosition.xz;

            NativeList<TriangleFlattenIndexBufferElement> triangleIds =
                new NativeList<TriangleFlattenIndexBufferElement>(64, Allocator.Temp);
            int triangleIdSize = Common.GetTriangleIdsByPosition(ref triangleIds, agentPosition,
                this.cellXLength,
                this.cellZLength, this.minFloorX, this.minFloorZ, this.groupDivision, this.triangleIndexes,
                this.triangleArraySizes);


            if (triangleIdSize == 0)
                triangleIdSize = Common.GetTriangleIdsByPositionSpiralOutwards(ref triangleIds,
                    agentPosition, 1, 2, 1,
                    this.cellXLength,
                    this.cellZLength, this.minFloorX, this.minFloorZ, this.groupDivision, this.triangleIndexes,
                    this.triangleArraySizes);

            //Try to find the triangle the agent is already within
            for (int i = 0; i < triangleIdSize; i++)
            {
                TriangleFlattenIndexBufferElement triangleId = triangleIds[i];
                NavTriangleBufferElement t = this.triangles[triangleId.Index];

                if (t.minBound.x > agentPosition2D.x || t.minBound.y > agentPosition2D.y ||
                    t.maxBound.x < agentPosition2D.x || t.maxBound.y < agentPosition2D.y)
                    continue;

                if (!MathC.PointWithinTriangle2D(agentPosition2D, this.verts[t.A].XZ(),
                        this.verts[t.B].XZ(),
                        this.verts[t.C].XZ()))
                    continue;

                agent.CurrentTriangleID = triangleId.Index;
                return;
            }

            if (triangleIds.Length == 0)
            {
                agent.CurrentTriangleID = -1;
                return;
            }

            int closestID = this.triangles[triangleIds[0].Index].ID;
            float dist = agentPosition.QuickSquareDistance(this.triangles[triangleIds[0].Index].Center) -
                         this.triangles[closestID].SquaredRadius;

            foreach (TriangleFlattenIndexBufferElement triangleId in triangleIds)
            {
                NavTriangleBufferElement t = this.triangles[triangleId.Index];
                float d = agentPosition.QuickSquareDistance(t.Center) - t.SquaredRadius;
                if (d > dist)
                    continue;

                dist = d;
                closestID = t.ID;
            }

            agent.CurrentTriangleID = closestID;
        }
    }
}