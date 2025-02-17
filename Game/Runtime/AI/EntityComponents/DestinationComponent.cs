using Unity.Entities;
using Unity.Mathematics;

namespace Runtime.AI.EntityComponents
{
    public struct DestinationComponent : IComponentData, IEnableableComponent
    {
        public bool Debug;
        public bool Stop;
        public float3 Point;
        public int TriangleID;
        public float2 IntendedDirection;
    }
}