#region Packages

using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.VFX.Reuseable;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.Core
{
    public class RayHitHolder : MonoBehaviour
    {
        #region Values

        [FoldoutGroup("Contains")] [SerializeField]
        private bool hasComponents, hasTags, hasImpactEffects;

        [FoldoutGroup("Contains"), ShowIf("hasTags")] [SerializeField]
        private string[] tags;

        [FoldoutGroup("Impact Effects"), ShowIf("hasImpactEffects")]
        private ReuseableEffect[] impactEffects;

        private readonly List<MonoBehaviour> allComponents = new List<MonoBehaviour>();

        private void OnValidate()
        {
            if (!this.hasTags) this.tags = Array.Empty<string>();
            if (!this.hasImpactEffects) this.impactEffects = Array.Empty<ReuseableEffect>();

            this.tags = this.tags.Where(s => !s.Replace(" ", "").Equals("")).ToArray();
            this.impactEffects = this.impactEffects.Where(e => e is not null).ToArray();
        }

        #endregion

        #region Build In States

        private void Awake()
        {
            if (this.hasComponents)
                this.allComponents.AddRange(this.gameObject.GetComponents<MonoBehaviour>());
        }

        #endregion

        #region Out

        public bool HasComponents()
        {
            return this.hasComponents;
        }

        public bool ContainsTag(string tagReference)
        {
            return this.tags.Contains(tagReference);
        }

        public bool HasTags()
        {
            return this.hasTags;
        }

        public MonoBehaviour[] GetAllComponents()
        {
            return this.allComponents.ToArray();
        }

        public T[] GetComponentsOfType<T>()
            where T : MonoBehaviour
        {
            return this.allComponents.OfType<T>().ToArray();
        }

        #endregion
    }
}