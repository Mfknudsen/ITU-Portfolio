#region Libraries

using Runtime.Systems.Pooling;
using System.Collections.Generic;
using System.Linq;
using Runtime.VFX.SingleUse;
using UnityEngine;

#endregion

namespace Runtime.VFX
{
    public class SingleUseEffectHolder
    {
        private readonly int maxCount;
        private readonly List<SingleUseEffect> actives, disabled;

        private readonly bool multiplyPrefabs;

        private readonly SingleUseEffect prefab;
        private readonly SingleUseEffect[] prefabs;

        public SingleUseEffectHolder(int maxCount, SingleUseEffect prefab)
        {
            this.multiplyPrefabs = false;

            this.maxCount = maxCount;
            this.prefab = prefab;
            this.disabled = new List<SingleUseEffect>();
            this.actives = new List<SingleUseEffect>();
        }

        private int TotalCount => this.disabled.Count + this.actives.Count;

        #region In

        public void Add(SingleUseEffect worldEffect)
        {
            if (worldEffect.gameObject.activeInHierarchy)
                this.actives.Add(worldEffect);
            else
                this.disabled.Add(worldEffect);
        }

        public void Remove(SingleUseEffect worldEffect)
        {
            if (this.actives.Contains(worldEffect))
                this.actives.Remove(worldEffect);
            else if (this.disabled.Contains(worldEffect))
                this.disabled.Remove(worldEffect);
        }

        public void CheckActives()
        {
            foreach (SingleUseEffect singleUseEffect in this.actives
                         .Where(singleUseEffect => singleUseEffect.CheckActive()))
                this.Switch(singleUseEffect);
        }

        public void UpdateEffects()
        {
            foreach (SingleUseEffect singleUseEffect in this.actives)
                singleUseEffect.UpdateEffect();
        }

        #endregion

        #region Out

        public SingleUseEffect TryGetEffect()
        {
            SingleUseEffect selected;

            if (this.TotalCount == this.maxCount && this.disabled.Count == 0)
            {
                selected = this.actives[0];
                this.actives.RemoveAt(0);
                this.actives.Add(selected);
                selected.ResetEffect();
                return selected;
            }

            if ((this.TotalCount < this.maxCount || this.maxCount == 0) && this.disabled.Count == 0)
            {
                selected = Pool.Create((!this.multiplyPrefabs
                    ? this.prefab
                    : this.prefabs[Random.Range(0, this.prefabs.Length)]), activate: true)
                    .GetComponent<SingleUseEffect>();

                this.Add(selected);
                selected.ResetEffect();
                return selected;
            }

            if (this.disabled.Count == 0) return null;

            selected = this.disabled[0];
            this.disabled.RemoveAt(0);
            this.actives.Add(selected);
            selected.ResetEffect();
            return selected;
        }

        #endregion

        #region Internal

        private void Switch(SingleUseEffect worldEffect)
        {
            if (worldEffect.IsActive)
            {
                this.actives.Add(worldEffect);
                this.disabled.Remove(worldEffect);
            }
            else
            {
                this.disabled.Add(worldEffect);
                this.actives.Remove(worldEffect);
            }
        }

        #endregion
    }
}