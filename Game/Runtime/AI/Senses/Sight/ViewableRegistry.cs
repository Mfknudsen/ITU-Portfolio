#region Packages

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

namespace Runtime.AI.Senses.Sight
{
    [CreateAssetMenu(menuName = "AI/View/View Registry")]
    public sealed class ViewableRegistry : ScriptableObject
    {
        #region Values

        private readonly List<Viewable> registeredViewable = new List<Viewable>();

        #endregion

        #region In

        public void Register(Viewable viewable) =>
            this.registeredViewable.Add(viewable);

        public void Unregister(Viewable viewable) =>
            this.registeredViewable.Remove(viewable);

        #endregion

        #region Out

        public List<Viewable> GetViewableInDistance(Vector3 position, float radius) =>
            this.registeredViewable
                .Where(v => (position - v.GetPosition).sqrMagnitude - v.GetCheckRadius * v.GetCheckRadius <
                            radius * radius)
                .ToList();

        #endregion
    }
}