using Unity.Entities;

namespace Runtime.AI.EntityBuffers
{
    public struct VertInTrianglesFlattenBufferElement : IBufferElementData
    {
        public int Size, StartIndex;
    }
}