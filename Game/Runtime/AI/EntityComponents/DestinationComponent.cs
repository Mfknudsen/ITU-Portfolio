using Unity.Entities;
using Unity.Mathematics;

namespace Runtime.AI.EntityComponents
{
    internal struct DestinationComponent : IComponentData, IEnableableComponent
    {
        public bool Debug;
        public bool Stop, Refresh, PositionWasUpdated;
        public float3 Point { get; private set; }
        public int TriangleID;

        public int TrianglePathCount, FunnelPathCount;

        public int CurrentPathIndex;
        public float3 MoveDirection;

        public void SetPoint(float3 set)
        {
            if (set.Equals(this.Point))
                return;

            this.Point = set;
            this.PositionWasUpdated = true;
        }
    }
}