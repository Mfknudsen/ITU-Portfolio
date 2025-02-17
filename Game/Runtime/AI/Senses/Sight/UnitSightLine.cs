#region Packages

using System.Linq;
using Runtime.Core;
using UnityEngine;

#endregion

namespace Runtime.AI.Senses.Sight
{
    public class UnitSightLine : UnitSight
    {
        private float effectiveRadius;

        protected void Start()
        {
            this.effectiveRadius =
                (this.originTransform.forward * this.radius + this.originTransform.right * this.size).magnitude;
        }

        private void OnDrawGizmosSelected()
        {
            if (this.originTransform == null) return;

            Gizmos.color = Color.white;

            Vector3 position = this.originTransform.position,
                forward = this.originTransform.forward,
                right = this.originTransform.right;

            Gizmos.DrawRay(position + right * this.size, -right * this.size * 2);
            Gizmos.DrawRay(position - right * this.size, forward * this.radius);
            Gizmos.DrawRay(position + right * this.size, forward * this.radius);
            Gizmos.DrawRay(position + right * this.size + forward * this.radius, -right * this.size * 2);
        }

        public override void UpdateSight()
        {
            Vector3 position = this.originTransform.position,
                forward = this.originTransform.forward;

            foreach (Viewable viewable in this.registry.GetViewableInDistance(position, this.effectiveRadius))
            {
                bool notVisible = true;

                foreach (Vector3 viewablePosition in viewable.GetViewableTransforms.Select(t => t.position))
                {
                    Vector3 towards = viewablePosition - position;
                    if (Vector3.Angle(forward, towards) > 90)
                        continue;

                    if (viewablePosition.ShortDistancePointToLine(position, position + forward) > this.size)
                        continue;

                    notVisible = false;

                    break;
                }

                if (notVisible) continue;

                break;
            }
        }
    }
}