using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Runtime.AI.EntitySystems
{
    [UpdateInGroup(typeof(NavigationSystemGroup))]
    [BurstCompile]
    internal partial struct SetRemappingDirectionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NavigationMeshSingletonComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity navmeshEntity = SystemAPI.GetSingletonEntity<NavigationMeshSingletonComponent>();

            DynamicBuffer<TriangleFlattenBufferElement> sizeBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenBufferElement>(navmeshEntity);

            if (sizeBufferElements.Length == 0)
                return;

            DynamicBuffer<TriangleFlattenIndexBufferElement> triangleIndexBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenIndexBufferElement>(navmeshEntity);

            NavigationMeshSingletonComponent navmesh = SystemAPI.GetSingleton<NavigationMeshSingletonComponent>();

            DynamicBuffer<VertBufferElement> verts =
                SystemAPI.GetBuffer<VertBufferElement>(navmeshEntity);
            DynamicBuffer<NavTriangleBufferElement> triangles =
                SystemAPI.GetBuffer<NavTriangleBufferElement>(navmeshEntity);

            SetRemappingDirectionJob setRemappingDirectionJob = new SetRemappingDirectionJob(
                navmesh.GroupDivision,
                navmesh.MinFloorX,
                navmesh.MinFloorZ,
                navmesh.CellXLength,
                navmesh.CellZLength,
                triangles,
                verts,
                sizeBufferElements,
                triangleIndexBufferElements);

            state.Dependency = setRemappingDirectionJob.Schedule(verts.Length, 128, state.Dependency);
        }
    }

    [BurstCompile]
    public struct SetRemappingDirectionJob : IJobParallelFor
    {
        [ReadOnly] private readonly float groupDivision, minFloorX, minFloorZ;

        [ReadOnly] private readonly int cellXLength, cellZLength;

        [DeallocateOnJobCompletion] [ReadOnly] private readonly NativeArray<NavTriangleBufferElement> triangles;

        [DeallocateOnJobCompletion] [ReadOnly]
        private readonly NativeArray<TriangleFlattenBufferElement> sizeBufferElements;

        [DeallocateOnJobCompletion] [ReadOnly]
        private readonly NativeArray<TriangleFlattenIndexBufferElement> triangleIndexBufferElements;

        [NativeDisableParallelForRestriction] private DynamicBuffer<VertBufferElement> simpleVerts;

        public SetRemappingDirectionJob(float groupDivision, float minFloorX, float minFloorZ,
            int cellXLength, int cellZLength,
            DynamicBuffer<NavTriangleBufferElement> triangles,
            DynamicBuffer<VertBufferElement> simpleVerts,
            DynamicBuffer<TriangleFlattenBufferElement> sizeBufferElements,
            DynamicBuffer<TriangleFlattenIndexBufferElement> triangleIndexBufferElements)
        {
            this.triangles = triangles.ToNativeArray(Allocator.TempJob);
            this.simpleVerts = simpleVerts;

            this.groupDivision = groupDivision;
            this.minFloorX = minFloorX;
            this.minFloorZ = minFloorZ;

            this.sizeBufferElements = sizeBufferElements.ToNativeArray(Allocator.TempJob);
            this.triangleIndexBufferElements = triangleIndexBufferElements.ToNativeArray(Allocator.TempJob);

            this.cellXLength = cellXLength;
            this.cellZLength = cellZLength;
        }

        [BurstCompile]
        public void Execute(int index)
        {
            float3 direction = float3.zero;

            NativeList<TriangleFlattenIndexBufferElement> triangleIds =
                new NativeList<TriangleFlattenIndexBufferElement>(16, Allocator.Temp);
            int triangleIdsSize = Common.GetTriangleIdsByPosition(
                ref triangleIds,
                this.simpleVerts[index].Position.x,
                this.simpleVerts[index].Position.z, this.cellXLength, this.cellZLength, this.minFloorX, this.minFloorZ,
                this.groupDivision,
                this.triangleIndexBufferElements, this.sizeBufferElements);
        }
    }
}