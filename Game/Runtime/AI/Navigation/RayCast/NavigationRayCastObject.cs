#region Libraries

using UnityEngine;

#endregion

namespace Runtime.AI.Navigation.RayCast
{
    public abstract class NavigationRayCastObject
    {
        #region In

        public abstract bool Check(Vector2 origin, Vector2 direction, float radius, out Vector2 hit);

        #endregion
    }
}