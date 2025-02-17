#region Libraries

using Unity.Entities;

#endregion

namespace Runtime.AI.EntityComponents
{
    public struct UnitAgentComponent : IComponentData
    {
        #region Values

        public int ID;
        public int CurrentTriangleID;

        #endregion
    }
}