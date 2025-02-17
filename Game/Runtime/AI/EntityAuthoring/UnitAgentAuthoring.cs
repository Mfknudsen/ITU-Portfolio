#region Libraries

using Unity.Entities;
using UnityEngine;

#endregion

namespace Runtime.AI.EntityAuthoring
{
    public sealed class UnitAgentAuthoring : MonoBehaviour
    {
        #region Values

        #endregion

        #region Build In States

        private class Baker : Baker<UnitAgentAuthoring>
        {
            public override void Bake(UnitAgentAuthoring authoring)
            {
                Entity entity = this.GetEntity(TransformUsageFlags.Dynamic);
            }
        }

        #endregion
    }
}