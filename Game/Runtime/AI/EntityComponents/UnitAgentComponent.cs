#region Libraries

using Unity.Entities;
using Unity.Mathematics;

#endregion

namespace Runtime.AI.EntityComponents
{
    public struct UnitAgentComponent : IComponentData
    {
        public int ID;
        public int CurrentTriangleID;
        public float3 Position;
        public quaternion Rotation;
    }
}