#region Libraries

using Unity.Entities;

#endregion

namespace Runtime.AI.EntityComponents
{
    public struct NavigationMeshSingletonComponent : IComponentData
    {
        #region Values

        public float MinFloorX,
            MinFloorZ,
            MaxFloorX,
            MaxFloorZ;

        public float GroupDivision;

        public int CellXLength, CellZLength;

        #endregion
    }
}