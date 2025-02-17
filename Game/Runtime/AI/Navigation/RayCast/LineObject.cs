#region Libraries

using Runtime.Core;
using UnityEngine;

#endregion

namespace Runtime.AI.Navigation.RayCast
{
    public sealed class LineObject : NavigationRayCastObject
    {
        #region Values

        private readonly Vector2 pointA, pointB, normal;

        #endregion

        #region Build In States

        public LineObject(Vector2 pointA, Vector2 pointB, Vector2 normal)
        {
            this.pointA = pointA;
            this.pointB = pointB;
            this.normal = normal;
        }

        #endregion

        #region Getters

#if UNITY_EDITOR
        public Vector2 GetA()
        {
            return this.pointA;
        }

        public Vector2 GetB()
        {
            return this.pointB;
        }
#endif

        #endregion

        #region Out

        public override bool Check(Vector2 origin, Vector2 direction, float radius, out Vector2 hit)
        {
            if (MathC.LineIntersect2D(origin, origin + direction,
                    this.pointA + this.normal * radius,
                    this.pointB + this.normal * radius))
            {
                hit = MathC.ClosetPointOnLine(origin,
                    this.pointA + this.normal * radius,
                    this.pointB + this.normal * radius);
                return true;
            }

            hit = Vector2.zero;
            return false;
        }

        #endregion
    }
}