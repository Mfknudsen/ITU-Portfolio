#region Libraries

using Unity.Entities;
using Unity.Mathematics;

#endregion

namespace Runtime.AI.Navigation
{
    public struct UnitPathEcs
    {
        #region Values

        public int WalkIndex, InteractIndex;
        public DynamicBuffer<bool> ActionIsWalkingIndex;
        public DynamicBuffer<float3> Waypoints;

        #endregion

        #region Build In States

        public UnitPathEcs(DynamicBuffer<bool> actionIsWalkingIndex, DynamicBuffer<float3> waypoints)
        {
            this.WalkIndex = 0;
            this.InteractIndex = 0;

            this.ActionIsWalkingIndex = actionIsWalkingIndex;
            this.Waypoints = waypoints;
        }

        #endregion
    }
}