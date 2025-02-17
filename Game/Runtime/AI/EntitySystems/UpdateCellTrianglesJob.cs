using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Runtime.AI.EntitySystems
{
    [BurstCompile]
    public partial struct UpdateCellTrianglesJob : IJobEntity
    {
        [ReadOnly] private readonly float groupDivision;

        [ReadOnly] private readonly float minFloorX, minFloorZ;

        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<VertXZBufferElement> simpleVerts;
        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<NavTriangleBufferElement> triangles;

        public UpdateCellTrianglesJob(float groupDivision,
            float minFloorX,
            float minFloorZ,
            NativeArray<VertXZBufferElement> simpleVerts,
            NativeArray<NavTriangleBufferElement> triangles) : this()
        {
            this.groupDivision = groupDivision;
            this.minFloorX = minFloorX;
            this.minFloorZ = minFloorZ;
            this.simpleVerts = simpleVerts;
            this.triangles = triangles;
        }

        public void Execute(ref DynamicBuffer<NavMeshCellTriangleIndexBufferElement> cellTriangles,
            in NavMeshCellComponent cellComponent)
        {
            float xMin = this.minFloorX + this.groupDivision * cellComponent.X,
                xMax = xMin + this.groupDivision,
                zMin = this.minFloorZ + this.groupDivision * cellComponent.Z,
                zMax = zMin + this.groupDivision;

            foreach (NavTriangleBufferElement navTriangleBufferElement in this.triangles)
            {
                if (!navTriangleBufferElement.CellNeedsUpdate)
                    continue;

                foreach (int vertexIndex in navTriangleBufferElement.Vertices())
                {
                    VertXZBufferElement point = this.simpleVerts[vertexIndex];

                    if (point.X >= xMin && point.X <= xMax && point.Z >= zMin && point.Z <= zMax)
                    {
                        if (!ListContains(cellTriangles, navTriangleBufferElement.ID))
                            cellTriangles.Add(new NavMeshCellTriangleIndexBufferElement
                                { Index = navTriangleBufferElement.ID });
                        break;
                    }

                    if (ListContains(cellTriangles, navTriangleBufferElement.ID, out int index))
                        cellTriangles.RemoveAt(index);
                }
            }
        }

        private static bool ListContains(DynamicBuffer<NavMeshCellTriangleIndexBufferElement> cellTriangles, int index)
        {
            foreach (NavMeshCellTriangleIndexBufferElement navMeshCellTriangleIndexBufferElement in cellTriangles)
            {
                if (navMeshCellTriangleIndexBufferElement.Index == index)
                    return true;
            }

            return false;
        }

        private static bool ListContains(DynamicBuffer<NavMeshCellTriangleIndexBufferElement> cellTriangles, int index,
            out int i)
        {
            i = 0;
            while (i < cellTriangles.Length)
            {
                if (cellTriangles[i].Index == index)
                    return true;

                i++;
            }

            return false;
        }
    }
}