using Unity.Entities;

namespace Runtime.AI.EntityComponents
{
    public struct NavMeshCellComponent : IComponentData
    {
        public int X, Z;
    }
}