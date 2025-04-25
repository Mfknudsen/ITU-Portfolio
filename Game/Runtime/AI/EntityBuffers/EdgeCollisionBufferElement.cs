using Unity.Entities;
using Unity.Mathematics;

namespace Runtime.AI.EntityBuffers
{
    public struct EdgeCollisionBufferElement : IBufferElementData
    {
        public float3 Start, End;
        public float3 StartOffset, EndOffset;
        public float3 StartEnd;
    }
}