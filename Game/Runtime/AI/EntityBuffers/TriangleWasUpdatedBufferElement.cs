using Unity.Entities;

namespace Runtime.AI.EntityBuffers
{
    public struct TriangleWasUpdatedBufferElement : IBufferElementData
    {
        public int Index;

        public TriangleWasUpdatedBufferElement(int index)
        {
            this.Index = index;
        }
    }
}