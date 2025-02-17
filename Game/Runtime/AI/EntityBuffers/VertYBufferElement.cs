using Unity.Entities;

namespace Runtime.AI.EntityBuffers
{
    public struct VertYBufferElement : IBufferElementData
    {
        public float Y;
        public float RemappingYDir;
    }
}