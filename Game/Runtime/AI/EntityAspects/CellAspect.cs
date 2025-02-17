using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Unity.Entities;

namespace Runtime.AI.EntityAspects
{
    public readonly partial struct CellAspect : IAspect
    {
        public readonly RefRO<NavMeshCellComponent> Cell;
        public readonly DynamicBuffer<NavMeshCellTriangleIndexBufferElement> Buffer;
    }
}