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
    public partial struct AgentInTriangleJob : IJobEntity
    {
        [ReadOnly] private readonly float groupDivision;

        [ReadOnly] private readonly int cellXLength, cellZLength;
        [ReadOnly] private readonly float minFloorX, minFloorZ;

        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<VertYBufferElement> vertsY;
        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<VertXZBufferElement> simpleVerts;
        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<NavTriangleBufferElement> triangles;

        //Used to flatten a 3D array 
        [DeallocateOnJobCompletion] [ReadOnly]
        private readonly NativeArray<TriangleFlattenSizeBufferElement> triangleArraySizes;

        [DeallocateOnJobCompletion] [ReadOnly]
        private readonly NativeArray<TriangleFlattenStartIndexBufferElement> triangleStartIndexArray;

        [DeallocateOnJobCompletion] [ReadOnly]
        private readonly NativeArray<TriangleFlattenIndexBufferElement> triangleIndexes;

        public AgentInTriangleJob(float groupDivision, float minFloorX, float minFloorZ, int cellXLength,
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
        public void Execute(ref UnitAgentComponent agent, in LocalTransform transform,
            in AgentSettingsComponent settings)
        {
            float3 agentPosition = transform.Position.xyz;
            float2 agentPosition2D = agentPosition.xz;

            NativeArray<int> triangleIds = Common.GetTriangleIdsByPosition(agentPosition, this.cellXLength,
                this.cellZLength, this.minFloorX, this.minFloorZ, this.groupDivision, this.triangleIndexes,
                this.triangleStartIndexArray, this.triangleArraySizes);

            if (triangleIds.Length == 0)
                triangleIds = Common.GetTriangleIdsByPositionSpiralOutwards(agentPosition, 1, 2, 1, this.cellXLength,
                    this.cellZLength, this.minFloorX, this.minFloorZ, this.groupDivision, this.triangleIndexes,
                    this.triangleStartIndexArray, this.triangleArraySizes);

            //Try to find the triangle the agent is already within
            foreach (int triangleId in triangleIds)
            {
                NavTriangleBufferElement t = this.triangles[triangleId];

                if (t.minBound.x > agentPosition2D.x || t.minBound.y > agentPosition2D.y ||
                    t.maxBound.x < agentPosition2D.x || t.maxBound.y < agentPosition2D.y)
                    continue;

                if (!MathC.PointWithinTriangle2D(agentPosition2D, this.simpleVerts[t.A].XZ(),
                        this.simpleVerts[t.B].XZ(),
                        this.simpleVerts[t.C].XZ()))
                    continue;

                agent.CurrentTriangleID = triangleId;
                return;
            }

            int closestID = this.triangles[triangleIds[0]].ID;
            float dist = agentPosition.QuickSquareDistance(this.triangles[triangleIds[0]].Center) -
                         this.triangles[closestID].SquaredRadius;

            foreach (int triangleId in triangleIds)
            {
                NavTriangleBufferElement t = this.triangles[triangleId];
                float d = agentPosition.QuickSquareDistance(t.Center) - t.SquaredRadius;
                if (d > dist)
                    continue;

                dist = d;
                closestID = t.ID;
            }

            agent.CurrentTriangleID = closestID;

            /*
            //Try to find the triangle the agent is within
            foreach (int i in triangleIds)
            {
                NavTriangleBufferElement t = this.triangles[i];

                if (!MathC.PointWithinTriangle2D(agentPosition2D, this.simpleVerts[t.A].XZ(),
                        this.simpleVerts[t.B].XZ(),
                        this.simpleVerts[t.C].XZ()))
                    continue;

                agent.CurrentTriangleID = i;
                return;
            }

            foreach (int triangleId in triangleIds)
            {
                NavTriangleBufferElement t = this.triangles[triangleId];
                MathC.PointWithinTriangle2D(agentPosition2D, this.simpleVerts[t.A].XZ(), this.simpleVerts[t.B].XZ(),
                    this.simpleVerts[t.C].XZ());
            }

            int result = triangleIds[0];

            int firstVertexIndex = this.triangles[triangleIds[0]].A;

            float firstDist =
                agentPosition.QuickSquareDistance(Common.VertByIndex(firstVertexIndex, this.simpleVerts, this.vertsY));

            foreach (int id in triangleIds)
            foreach (int vertex in this.triangles[id].Vertices())
            {
                if (firstVertexIndex == vertex)
                    continue;

                float dist =
                    agentPosition.QuickSquareDistance(Common.VertByIndex(vertex, this.simpleVerts, this.vertsY));

                if (dist > firstDist) continue;

                firstDist = dist;
                firstVertexIndex = vertex;
                result = id;
            }

            agent.CurrentTriangleID = result;
            */
        }
    }
}