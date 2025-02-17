#region Libraries

using Runtime.Core;
using UnityEngine;

#endregion

namespace Runtime.AI.Navigation.RayCast
{
    public sealed class CircleObject : NavigationRayCastObject
    {
        #region Values

        private readonly Vector2 position;

        private readonly float size;

        #endregion

        #region Build In States

        public CircleObject(Vector2 position, float size)
        {
            this.position = position;
            this.size = size;
        }

        #endregion

        #region Getter

#if UNITY_EDITOR
        public Vector2 GetPosition()
        {
            return this.position;
        }

        public float GetRadius()
        {
            return this.size;
        }
#endif

        #endregion

        #region Out

        public override bool Check(Vector2 origin, Vector2 direction, float radius, out Vector2 hit)
        {
            return MathC.ClosestPointLineIntersectCircle(origin, origin + direction, this.position, this.size + radius,
                out hit);
        }

        #endregion
    }
}