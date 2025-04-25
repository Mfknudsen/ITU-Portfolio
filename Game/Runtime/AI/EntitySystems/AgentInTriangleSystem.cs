using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Runtime.Core;
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
    internal partial struct AgentInTriangleSystem : ISystem
    {
        private EntityQuery entityQuery;

        private ComponentLookup<UnitAgentComponent> agentLookup;
        private ComponentLookup<AgentSettingsComponent> settingLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NavigationMeshSingletonComponent>();
            state.RequireForUpdate<NavigationMeshSingletonComponent>();

            this.entityQuery = state.GetEntityQuery(ComponentType.ReadWrite<UnitAgentComponent>());

            this.agentLookup = state.GetComponentLookup<UnitAgentComponent>();
            this.settingLookup = state.GetComponentLookup<AgentSettingsComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.agentLookup.Update(ref state);
            this.settingLookup.Update(ref state);

            if (!SystemAPI.TryGetSingletonEntity<NavigationMeshSingletonComponent>(out Entity navmeshEntity))
                return;

            NativeArray<Entity> entities = this.entityQuery
                .ToEntityArray(Allocator.TempJob);

            if (entities.Length == 0)
            {
                state.Dependency = entities.Dispose(state.Dependency);
                return;
            }

            NativeArray<TriangleFlattenBufferElement> size =
                SystemAPI.GetBuffer<TriangleFlattenBufferElement>(navmeshEntity).AsNativeArray();

            JobHandle sizeHandle;
            if (size.Length == 0)
            {
                sizeHandle = size.Dispose(state.Dependency);
                state.Dependency = entities.Dispose(sizeHandle);
                return;
            }

            NavigationMeshSingletonComponent navmeshSingletonComponent =
                SystemAPI.GetSingleton<NavigationMeshSingletonComponent>();
            NativeArray<TriangleFlattenIndexBufferElement> triangleIndex =
                SystemAPI.GetBuffer<TriangleFlattenIndexBufferElement>(navmeshEntity).AsNativeArray();
            NativeArray<VertBufferElement> verts =
                SystemAPI.GetBuffer<VertBufferElement>(navmeshEntity).AsNativeArray();
            NativeArray<NavTriangleBufferElement> triangles =
                SystemAPI.GetBuffer<NavTriangleBufferElement>(navmeshEntity).AsNativeArray();

            int batch = math.max(entities.Length / SystemInfo.processorCount, 1);

            AgentInTriangleJob agentInTriangleJob = new AgentInTriangleJob(
                entities,
                navmeshSingletonComponent.GroupDivision,
                navmeshSingletonComponent.MinFloorX,
                navmeshSingletonComponent.MinFloorZ,
                navmeshSingletonComponent.CellXLength,
                navmeshSingletonComponent.CellZLength,
                verts,
                triangles,
                size,
                triangleIndex,
                this.agentLookup,
                this.settingLookup);

            JobHandle handle = agentInTriangleJob.ScheduleParallel(entities.Length, batch, state.Dependency);
            sizeHandle = size.Dispose(handle);
            JobHandle triangleIndexHandle = triangleIndex.Dispose(sizeHandle);
            JobHandle vertsHandle = verts.Dispose(triangleIndexHandle);
            JobHandle trianglesHandle = triangles.Dispose(vertsHandle);
            state.Dependency = entities.Dispose(trianglesHandle);
        }
    }

    [BurstCompile]
    internal struct AgentInTriangleJob : IJobFor
    {
        [ReadOnly] private readonly float groupDivision;

        [ReadOnly] private readonly int cellXLength, cellZLength;
        [ReadOnly] private readonly float minFloorX, minFloorZ;

        [ReadOnly] private NativeArray<VertBufferElement> verts;
        [ReadOnly] private NativeArray<NavTriangleBufferElement> triangles;

        //Used to flatten a 3D array 
        [ReadOnly] private readonly NativeArray<TriangleFlattenBufferElement> triangleArraySizes;

        [ReadOnly] private readonly NativeArray<TriangleFlattenIndexBufferElement> triangleIndexes;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private NativeArray<Entity> entities;

        [NativeDisableParallelForRestriction] private ComponentLookup<UnitAgentComponent> agentLookup;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private ComponentLookup<AgentSettingsComponent> settingLookup;

        public AgentInTriangleJob(
            NativeArray<Entity> entities,
            float groupDivision, float minFloorX, float minFloorZ, int cellXLength, int cellZLength,
            NativeArray<VertBufferElement> verts,
            NativeArray<NavTriangleBufferElement> triangles,
            NativeArray<TriangleFlattenBufferElement> sizeArray,
            NativeArray<TriangleFlattenIndexBufferElement> indexList,
            ComponentLookup<UnitAgentComponent> agentLookup,
            ComponentLookup<AgentSettingsComponent> settingLookup) : this()
        {
            this.entities = entities;

            this.groupDivision = groupDivision;

            this.minFloorX = minFloorX;
            this.minFloorZ = minFloorZ;

            this.cellXLength = cellXLength;
            this.cellZLength = cellZLength;

            this.verts = verts;
            this.triangles = triangles;

            this.triangleArraySizes = sizeArray;
            this.triangleIndexes = indexList;

            this.agentLookup = agentLookup;
            this.settingLookup = settingLookup;
        }

        [BurstCompile]
        public void Execute(int index)
        {
            Entity entity = this.entities[index];
            UnitAgentComponent agent = this.agentLookup[entity];

            float3 agentPosition = agent.Position.xyz;
            float2 agentPosition2D = agentPosition.xz;

            if (agent.CurrentTriangleID != -1)
            {
                NavTriangleBufferElement t = this.triangles[agent.CurrentTriangleID];

                if (MathC.PointWithinTriangle2D(agentPosition2D, this.verts[t.A].XZ(),
                        this.verts[t.B].XZ(),
                        this.verts[t.C].XZ()))
                {
                    return;
                }
            }

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

                if (triangleId.Index == agent.CurrentTriangleID)
                    continue;

                NavTriangleBufferElement t = this.triangles[triangleId.Index];

                if (t.MinBound.x > agentPosition2D.x || t.MinBound.y > agentPosition2D.y ||
                    t.MaxBound.x < agentPosition2D.x || t.MaxBound.y < agentPosition2D.y)
                    continue;

                if (!MathC.PointWithinTriangle2D(agentPosition2D, this.verts[t.A].XZ(),
                        this.verts[t.B].XZ(),
                        this.verts[t.C].XZ()))
                    continue;

                agent.CurrentTriangleID = triangleId.Index;
                this.agentLookup[entity] = agent;
                return;
            }

            if (triangleIds.Length == 0)
            {
                agent.CurrentTriangleID = -1;
                this.agentLookup[entity] = agent;
                return;
            }

            int closestID = this.triangles[triangleIds[0].Index].ID;
            float dist = agentPosition.QuickSquareDistance(this.triangles[triangleIds[0].Index].Center) -
                         this.triangles[closestID].SquaredRadius;

            foreach (TriangleFlattenIndexBufferElement triangleId in triangleIds)
            {
                if (triangleId.Index == agent.CurrentTriangleID)
                    continue;

                NavTriangleBufferElement t = this.triangles[triangleId.Index];
                float d = agentPosition.QuickSquareDistance(t.Center) - t.SquaredRadius;
                if (d > dist)
                    continue;

                dist = d;
                closestID = t.ID;
            }

            agent.CurrentTriangleID = closestID;
            this.agentLookup[entity] = agent;
        }
    }
}