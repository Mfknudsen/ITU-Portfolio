using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Unity.Entities;

namespace Runtime.AI.EntityAspects
{
    public readonly partial struct DebugCellAspect : IAspect
    {
        public readonly RefRO<NavMeshCellComponent> CellComponent;
        public readonly DynamicBuffer<NavMeshCellTriangleIndexBufferElement> NavTriangleBufferElements;
    }
}