using Unity.Entities;

namespace Runtime.AI.EntityBuffers
{
    public struct AgentTrianglePathBufferElement : IBufferElementData
    {
        public int Index;

        public AgentTrianglePathBufferElement(int index)
        {
            this.Index = index;
        }
    }
}