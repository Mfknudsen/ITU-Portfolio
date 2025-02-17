using Unity.Entities;
using Unity.Mathematics;

namespace Runtime.AI.EntityBuffers
{
    public struct WayPointBufferElement : IBufferElementData
    {
        public bool IsWalk;
        public float3 Point;
    }
}