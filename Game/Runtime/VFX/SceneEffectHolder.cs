#region Packages

using System.Collections.Generic;
using System.Linq;
using Runtime.Systems;
using Runtime.VFX.Scene;
using UnityEngine;

#endregion

namespace Runtime.VFX
{
    public class SceneEffectHolder
    {
        private readonly int maxCount;
        private readonly List<SceneEffect> actives, disabled;

        private readonly bool multiplyPrefabs;

        private readonly SceneEffect prefab;
        private readonly SceneEffect[] prefabs;

        private readonly EffectManager effectManager;

        public SceneEffectHolder(int maxCount, SceneEffect prefab, EffectManager effectManager)
        {
            this.effectManager = effectManager;
            this.multiplyPrefabs = false;

            this.maxCount = maxCount;
            this.prefab = prefab;
            this.disabled = new List<SceneEffect>();
            this.actives = new List<SceneEffect>();
        }

        public SceneEffectHolder(int maxCount, SceneEffect[] prefabs, EffectManager effectManager)
        {
            this.effectManager = effectManager;
            this.multiplyPrefabs = true;

            this.maxCount = maxCount;
            this.prefabs = prefabs;
            this.disabled = new List<SceneEffect>();
            this.actives = new List<SceneEffect>();
        }

        public int TotalCount => this.disabled.Count + this.actives.Count;

        public IEnumerable<SceneEffect> GetAll => this.disabled.Concat(this.actives).ToArray();

        public void Add(SceneEffect sceneEffect)
        {
            if (sceneEffect.gameObject.activeInHierarchy)
                this.actives.Add(sceneEffect);
            else
                this.disabled.Add(sceneEffect);
        }

        public void Remove(SceneEffect sceneEffect)
        {
            if (this.actives.Contains(sceneEffect))
                this.actives.Remove(sceneEffect);
            else if (this.disabled.Contains(sceneEffect))
                this.disabled.Remove(sceneEffect);
        }

        private void Switch(SceneEffect sceneEffect)
        {
            if (sceneEffect.IsActive)
            {
                this.actives.Add(sceneEffect);
                this.disabled.Remove(sceneEffect);
            }
            else
            {
                this.disabled.Add(sceneEffect);
                this.actives.Remove(sceneEffect);
            }
        }

        public void UpdateEffects()
        {
            foreach (SceneEffect sceneEffect in this.actives)
                sceneEffect.UpdateEffect();
        }

        public void UpdateActives()
        {
            foreach (SceneEffect sceneEffect in this.actives)
                sceneEffect.CheckRules();
        }

        public SceneEffect TryGetEffect()
        {
            SceneEffect selected = null;

            if (this.TotalCount == this.maxCount && this.disabled.Count == 0)
            {
                selected = this.actives[0];
                this.actives.RemoveAt(0);
                this.actives.Add(selected);
            }

            if (this.TotalCount < this.maxCount && this.disabled.Count == 0)
            {
                selected = !this.multiplyPrefabs
                    ? Object.Instantiate(this.prefab)
                    : this.prefabs[Random.Range(0, this.prefabs.Length)];
                this.Add(selected);
            }

            if (this.disabled.Count == 0)
            {
                selected = this.disabled[0];
                this.disabled.RemoveAt(0);
                this.actives.Add(selected);
            }

            return selected;
        }
    }
}