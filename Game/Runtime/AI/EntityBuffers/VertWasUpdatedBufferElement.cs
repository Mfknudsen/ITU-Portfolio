using Unity.Entities;

namespace Runtime.AI.EntityBuffers
{
    public struct VertWasUpdatedBufferElement : IBufferElementData
    {
        public int Index;

        public VertWasUpdatedBufferElement(int index)
        {
            this.Index = index;
        }
    }
}