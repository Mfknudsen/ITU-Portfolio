using Unity.Entities;
using Unity.Mathematics;

namespace Runtime.AI.EntityBuffers
{
    public struct AgentPathCollisionBufferElement : IBufferElementData
    {
        public float3 Position;
        public float Height, Radius;
    }
}