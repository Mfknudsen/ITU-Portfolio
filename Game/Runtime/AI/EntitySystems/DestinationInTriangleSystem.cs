using System;
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
    internal partial struct DestinationInTriangleSystem : ISystem
    {
        private EntityQuery entityQuery;

        private ComponentLookup<DestinationComponent> destinationLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NavigationMeshSingletonComponent>();
            state.RequireForUpdate<NavigationMeshSingletonComponent>();

            this.entityQuery = state.GetEntityQuery(ComponentType.ReadOnly<UnitAgentComponent>());

            this.destinationLookup = state.GetComponentLookup<DestinationComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonEntity<NavigationMeshSingletonComponent>(out Entity navmeshEntity))
                return;

            NavigationMeshSingletonComponent navmeshSingletonComponent =
                SystemAPI.GetSingleton<NavigationMeshSingletonComponent>();
            DynamicBuffer<TriangleFlattenBufferElement> sizeBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenBufferElement>(navmeshEntity);
            DynamicBuffer<TriangleFlattenIndexBufferElement> triangleIndexBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenIndexBufferElement>(navmeshEntity);
            DynamicBuffer<VertBufferElement> verts =
                SystemAPI.GetBuffer<VertBufferElement>(navmeshEntity);
            DynamicBuffer<NavTriangleBufferElement> triangles =
                SystemAPI.GetBuffer<NavTriangleBufferElement>(navmeshEntity);

            if (sizeBufferElements.Length == 0)
                return;

            this.destinationLookup.Update(ref state);

            NativeArray<Entity> entities = this.entityQuery.ToEntityArray(Allocator.TempJob);

            if (entities.Length == 0)
            {
                state.Dependency = entities.Dispose(state.Dependency);
                return;
            }

            int batch = math.max(entities.Length / SystemInfo.processorCount, 1);

            DestinationInTriangleJob destinationInTriangleJob = new DestinationInTriangleJob(
                entities, navmeshSingletonComponent.GroupDivision, navmeshSingletonComponent.MinFloorX,
                navmeshSingletonComponent.MinFloorZ, navmeshSingletonComponent.CellXLength,
                navmeshSingletonComponent.CellZLength, verts, triangles, sizeBufferElements,
                triangleIndexBufferElements,
                this.destinationLookup);
            JobHandle handle = destinationInTriangleJob.ScheduleParallel(entities.Length, batch, state.Dependency);
            state.Dependency = entities.Dispose(handle);
        }
    }

    [BurstCompile]
    internal struct DestinationInTriangleJob : IJobFor, IDisposable
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
        private NativeArray<Entity> entities;

        [NativeDisableParallelForRestriction] private ComponentLookup<DestinationComponent> destinationLookup;

        public DestinationInTriangleJob(
            NativeArray<Entity> entities,
            float groupDivision, float minFloorX, float minFloorZ, int cellXLength, int cellZLength,
            DynamicBuffer<VertBufferElement> simpleVerts,
            DynamicBuffer<NavTriangleBufferElement> triangles,
            DynamicBuffer<TriangleFlattenBufferElement> sizeArray,
            DynamicBuffer<TriangleFlattenIndexBufferElement> indexList,
            ComponentLookup<DestinationComponent> destinationLookup) : this()
        {
            this.entities = entities;

            this.groupDivision = groupDivision;

            this.minFloorX = minFloorX;
            this.minFloorZ = minFloorZ;

            this.cellXLength = cellXLength;
            this.cellZLength = cellZLength;

            this.verts = simpleVerts.ToNativeArray(Allocator.TempJob);
            this.triangles = triangles.ToNativeArray(Allocator.TempJob);

            this.triangleArraySizes = sizeArray.ToNativeArray(Allocator.TempJob);
            this.triangleIndexes = indexList.ToNativeArray(Allocator.TempJob);

            this.destinationLookup = destinationLookup;
        }

        [BurstCompile]
        public void Execute(int index)
        {
            Entity entity = this.entities[index];
            DestinationComponent destination = this.destinationLookup[entity];

            if (!(destination.TriangleID == -1 || destination.PositionWasUpdated))
                return;

            float3 position = destination.Point;
            float2 position2D = position.xz;

            NativeList<TriangleFlattenIndexBufferElement> triangleIds =
                new NativeList<TriangleFlattenIndexBufferElement>(16, Allocator.Temp);
            int triangleIdsSize = Common.GetTriangleIdsByPosition(ref triangleIds, position,
                this.cellXLength, this.cellZLength, this.minFloorX, this.minFloorZ, this.groupDivision,
                this.triangleIndexes, this.triangleArraySizes);

            if (triangleIdsSize == 0)
                triangleIdsSize = Common.GetTriangleIdsByPositionSpiralOutwards(ref triangleIds,
                    position, 1, 2, 1,
                    this.cellXLength,
                    this.cellZLength, this.minFloorX, this.minFloorZ, this.groupDivision, this.triangleIndexes,
                    this.triangleArraySizes);

            foreach (TriangleFlattenIndexBufferElement i in triangleIds)
            {
                NavTriangleBufferElement t = this.triangles[i.Index];

                if (t.MinBound.x > position2D.x || t.MinBound.y > position2D.y ||
                    t.MaxBound.x < position2D.x || t.MaxBound.y < position2D.y)
                    continue;

                if (!MathC.PointWithinTriangle2D(position2D, this.verts[t.A].XZ(),
                        this.verts[t.B].XZ(),
                        this.verts[t.C].XZ()))
                    continue;

                destination.TriangleID = i.Index;
                this.destinationLookup[entity] = destination;
                return;
            }

            if (triangleIds.Length == 0)
            {
                destination.TriangleID = -1;
                this.destinationLookup[entity] = destination;
                return;
            }

            int closestID = this.triangles[triangleIds[0].Index].ID;
            float dist = position.QuickSquareDistance(this.triangles[triangleIds[0].Index].Center) -
                         this.triangles[closestID].SquaredRadius;

            foreach (TriangleFlattenIndexBufferElement triangleId in triangleIds)
            {
                NavTriangleBufferElement t = this.triangles[triangleId.Index];
                float d = position.QuickSquareDistance(t.Center) - t.SquaredRadius;
                if (d > dist)
                    continue;

                dist = d;
                closestID = t.ID;
            }

            destination.TriangleID = closestID;
            this.destinationLookup[entity] = destination;
        }

        public void Dispose()
        {
            this.entities.Dispose();
        }
    }
}