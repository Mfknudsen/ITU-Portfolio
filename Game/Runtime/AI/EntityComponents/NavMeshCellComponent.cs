using Unity.Entities;

namespace Runtime.AI.EntityComponents
{
    public struct NavMeshCellComponent : IComponentData
    {
        public int X, Z;

        public int TriangleSize, VertSize, CollisionSize, NewSize;

        public bool debug;
    }
}