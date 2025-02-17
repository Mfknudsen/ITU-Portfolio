using Unity.Entities;
using Unity.Mathematics;

namespace Runtime.AI.EntityBuffers
{
    public struct AgentColliderBufferElement : IBufferElementData
    {
        public float3 Position;
        public float Radius;
    }
}
