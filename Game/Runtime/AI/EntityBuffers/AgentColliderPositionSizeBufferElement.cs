using Unity.Entities;

namespace Runtime.AI.EntityBuffers
{
    public struct AgentColliderPositionSizeBufferElement : IBufferElementData
    {
        public int Position, Size;
    }
}
