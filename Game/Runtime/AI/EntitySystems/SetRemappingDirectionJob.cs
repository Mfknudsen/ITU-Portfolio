using Runtime.AI.EntityBuffers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Runtime.AI.EntitySystems
{
    [BurstCompile]
    public struct SetRemappingDirectionJob : IJobParallelFor
    {
        [ReadOnly] private readonly float groupDivision, minFloorX, minFloorZ;

        [ReadOnly] private readonly int cellXLength, cellZLength;

        [DeallocateOnJobCompletion] [ReadOnly] private readonly NativeArray<NavTriangleBufferElement> triangles;

        [DeallocateOnJobCompletion] [ReadOnly]
        private readonly NativeArray<TriangleFlattenSizeBufferElement> sizeBufferElements;

        [DeallocateOnJobCompletion] [ReadOnly]
        private readonly NativeArray<TriangleFlattenStartIndexBufferElement> startIndexBufferElements;

        [DeallocateOnJobCompletion] [ReadOnly]
        private readonly NativeArray<TriangleFlattenIndexBufferElement> triangleIndexBufferElements;

        [NativeDisableParallelForRestriction] private DynamicBuffer<VertXZBufferElement> simpleVerts;
        [NativeDisableParallelForRestriction] private DynamicBuffer<VertYBufferElement> vertsY;

        public SetRemappingDirectionJob(float groupDivision, float minFloorX, float minFloorZ,
            int cellXLength, int cellZLength,
            DynamicBuffer<NavTriangleBufferElement> triangles,
            DynamicBuffer<VertXZBufferElement> simpleVerts, DynamicBuffer<VertYBufferElement> vertsY,
            DynamicBuffer<TriangleFlattenSizeBufferElement> sizeBufferElements,
            DynamicBuffer<TriangleFlattenStartIndexBufferElement> startIndexBufferElements,
            DynamicBuffer<TriangleFlattenIndexBufferElement> triangleIndexBufferElements)
        {
            this.triangles = triangles.ToNativeArray(Allocator.TempJob);
            this.simpleVerts = simpleVerts;
            this.vertsY = vertsY;

            this.groupDivision = groupDivision;
            this.minFloorX = minFloorX;
            this.minFloorZ = minFloorZ;

            this.sizeBufferElements = sizeBufferElements.ToNativeArray(Allocator.TempJob);
            this.startIndexBufferElements = startIndexBufferElements.ToNativeArray(Allocator.TempJob);
            this.triangleIndexBufferElements = triangleIndexBufferElements.ToNativeArray(Allocator.TempJob);

            this.cellXLength = cellXLength;
            this.cellZLength = cellZLength;
        }

        [BurstCompile]
        public void Execute(int index)
        {
            float3 direction = float3.zero;

            NativeArray<int> triangles = Common.GetTriangleIdsByPosition(this.simpleVerts[index].X,
                this.simpleVerts[index].Z, this.cellXLength, this.cellZLength, this.minFloorX, this.minFloorZ,
                this.groupDivision,
                this.triangleIndexBufferElements, this.startIndexBufferElements, this.sizeBufferElements);
        }
    }
}