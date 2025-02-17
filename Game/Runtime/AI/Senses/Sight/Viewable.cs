#region Packages

using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.AI.Senses.Sight
{
    public sealed class Viewable : MonoBehaviour
    {
        #region Values

        [SerializeField, Required] private ViewableRegistry registry;

        [SerializeField] private float heightOffset;

        [SerializeField, Min(0)] private float checkRadius;

        [SerializeField] private Transform[] viewableTransforms;

        [SerializeField] private List<ViewableTag> tags;

        #endregion

        #region Build In States

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, .1f);

            Gizmos.DrawSphere(this.GetPosition, this.checkRadius);
        }

        private void OnEnable() =>
            this.registry.Register(this);

        private void OnDisable() =>
            this.registry.Unregister(this);

        #endregion

        #region Getters

        public float GetCheckRadius => this.checkRadius;

        public Transform[] GetViewableTransforms => this.viewableTransforms;

        #endregion

        #region Out

        public Vector3 GetPosition => this.transform.position + Vector3.up * this.heightOffset;

        public bool ContainsTag(ViewableTag compareTag) => this.tags.Any(t => t == compareTag);

        #endregion
    }
}