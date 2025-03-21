#region Libraries

using Unity.Entities;

#endregion

namespace Runtime.AI.EntityComponents
{
    public struct UnitAgentComponent : IComponentData
    {
        public int ID;
        public int CurrentTriangleID;
    }
}