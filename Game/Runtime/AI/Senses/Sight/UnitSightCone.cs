#region Packages

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

namespace Runtime.AI.Senses.Sight
{
    public sealed class UnitSightCone : UnitSight
    {
        #region Build In States

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green * new Color(1, 1, 1, .3f);

            foreach (Viewable viewable in this.inSight)
                Gizmos.DrawSphere(viewable.transform.position, 2f);

            if (this.originTransform == null) return;

            Gizmos.color = (Color.yellow + Color.red) / 2;

            Vector3 forward = this.originTransform.forward,
                position = this.originTransform.position;
            Vector3 leftMost = Quaternion.Euler(0, -this.size, 0) * forward,
                rightMost = Quaternion.Euler(0, this.size, 0) * forward;

            Gizmos.DrawRay(position, leftMost.normalized * this.radius);
            Gizmos.DrawRay(position, rightMost.normalized * this.radius);

            int count = Mathf.FloorToInt(this.size / 3) + 1;
            Vector3 previous = leftMost.normalized;
            for (int i = 1; i <= count; i++)
            {
                Vector3 current = Vector3.Lerp(leftMost, rightMost, 1f / count * i).normalized;

                Gizmos.DrawLine(position + previous * this.radius, position + current * this.radius);

                previous = current;
            }
        }

        #endregion

        #region Getters

        public IEnumerable<Viewable> GetInSight() =>
            this.inSight.ToArray();

        #endregion

        #region In

        public override void UpdateSight()
        {
            Vector3 position = this.originTransform.position,
                forward = this.originTransform.forward;

            Viewable[] toCheck = this.inSight.ToArray();
            foreach (Viewable viewable in toCheck)
            {
                if ((position - viewable.transform.position).sqrMagnitude -
                    viewable.GetCheckRadius * viewable.GetCheckRadius > this.radius * this.radius)
                {
                    this.inSight.Remove(viewable);
                    continue;
                }

                if (!viewable.GetViewableTransforms
                        .Any(viewableGetViewableTransform =>
                            Vector3.Angle(forward, viewableGetViewableTransform.position - position) < this.size))
                    this.inSight.Remove(viewable);
            }

            List<Viewable> inDistance = this.registry.GetViewableInDistance(position, this.radius);

            if (inDistance.Count == 0)
            {
                this.inSight.Clear();
                return;
            }

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (Viewable viewable in inDistance)
            {
                if (this.inSight.Contains(viewable)) continue;

                foreach (Transform viewTransform in viewable.GetViewableTransforms)
                {
                    Vector3 vec = viewTransform.position - position;

                    if (Vector3.Angle(forward, vec) > this.size) continue;

                    Debug.Log(viewTransform.name);
                    
                    this.inSight.Add(viewable);
                    
                    break;
                }
            }
        }

        #endregion
    }
}