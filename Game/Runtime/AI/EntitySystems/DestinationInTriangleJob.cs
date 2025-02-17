using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Runtime.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Runtime.AI.EntitySystems
{
    [BurstCompile]
    public partial struct DestinationInTriangleJob : IJobEntity
    {
        [ReadOnly] private readonly float groupDivision;

        [ReadOnly] private readonly int cellXLength, cellZLength;
        [ReadOnly] private readonly float minFloorX, minFloorZ;

        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<VertYBufferElement> vertsY;
        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<VertXZBufferElement> simpleVerts;
        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<NavTriangleBufferElement> triangles;

        //Used to flatten a 3D array 
        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<TriangleFlattenSizeBufferElement> triangleArraySizes;

        [DeallocateOnJobCompletion] [ReadOnly]
        private NativeArray<TriangleFlattenStartIndexBufferElement> triangleStartIndexArray;

        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<TriangleFlattenIndexBufferElement> triangleIndexes;

        public DestinationInTriangleJob(float groupDivision, float minFloorX, float minFloorZ, int cellXLength,
            int cellZLength,
            DynamicBuffer<VertYBufferElement> vertsY,
            DynamicBuffer<VertXZBufferElement> simpleVerts,
            DynamicBuffer<NavTriangleBufferElement> triangles,
            DynamicBuffer<TriangleFlattenSizeBufferElement> sizeArray,
            DynamicBuffer<TriangleFlattenStartIndexBufferElement> startIndexArray,
            DynamicBuffer<TriangleFlattenIndexBufferElement> indexList) : this()
        {
            this.groupDivision = groupDivision;

            this.minFloorX = minFloorX;
            this.minFloorZ = minFloorZ;

            this.cellXLength = cellXLength;
            this.cellZLength = cellZLength;

            this.vertsY = vertsY.ToNativeArray(Allocator.TempJob);
            this.simpleVerts = simpleVerts.ToNativeArray(Allocator.TempJob);
            this.triangles = triangles.ToNativeArray(Allocator.TempJob);

            this.triangleArraySizes = sizeArray.ToNativeArray(Allocator.TempJob);
            this.triangleStartIndexArray = startIndexArray.ToNativeArray(Allocator.TempJob);
            this.triangleIndexes = indexList.ToNativeArray(Allocator.TempJob);
        }

        [BurstCompile]
        public void Execute(ref DestinationComponent destination,
            in AgentSettingsComponent settings)
        {
            float3 position = destination.Point;
            float2 position2D = position.xz;

            NativeArray<int> triangleIds = Common.GetTriangleIdsByPosition(position,
                this.cellXLength, this.cellZLength, this.minFloorX, this.minFloorZ, this.groupDivision,
                this.triangleIndexes, this.triangleStartIndexArray, this.triangleArraySizes);

            if (triangleIds.Length == 0)
                triangleIds = Common.GetTriangleIdsByPositionSpiralOutwards(position, 1, 2, 1, this.cellXLength,
                    this.cellZLength, this.minFloorX, this.minFloorZ, this.groupDivision, this.triangleIndexes,
                    this.triangleStartIndexArray, this.triangleArraySizes);

            foreach (int i in triangleIds)
            {
                NavTriangleBufferElement t = this.triangles[i];

                if (t.minBound.x > position2D.x || t.minBound.y > position2D.y ||
                    t.maxBound.x < position2D.x || t.maxBound.y < position2D.y)
                    continue;

                if (!MathC.PointWithinTriangle2D(position2D, this.simpleVerts[t.A].XZ(),
                        this.simpleVerts[t.B].XZ(),
                        this.simpleVerts[t.C].XZ()))
                    continue;

                destination.TriangleID = i;
                return;
            }

            int closestID = this.triangles[triangleIds[0]].ID;
            float dist = position.QuickSquareDistance(this.triangles[triangleIds[0]].Center) -
                         this.triangles[closestID].SquaredRadius;

            foreach (int triangleId in triangleIds)
            {
                NavTriangleBufferElement t = this.triangles[triangleId];
                float d = position.QuickSquareDistance(t.Center) - t.SquaredRadius;
                if (d > dist)
                    continue;

                dist = d;
                closestID = t.ID;
            }

            destination.TriangleID = closestID;
        }
    }
}