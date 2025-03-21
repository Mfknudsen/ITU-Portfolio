using Unity.Entities;

namespace Runtime.AI.EntityBuffers
{
    public struct TriangleFlattenBufferElement : IBufferElementData
    {
        public int Size, StartIndex;
    }
}