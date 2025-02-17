#region Packages

using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

#endregion

namespace Runtime.VFX.SingleUse
{
    public abstract class SingleUseEffect : EffectBase
    {
        public bool IsActive { get; private set; }

        public virtual bool CheckActive() =>
            this.effectLoDs.GetByID(this.currentLevel).All(e => e.gameObject.activeSelf == false) && this.IsActive;

        public virtual void Play(Vector3 playerPosition)
        {
            this.IsActive = true;

            float sqrDistance = (playerPosition - this.effectTransform.position).sqrMagnitude;

            VisualEffect[] systems = this.effectLoDs.GetByDistance(sqrDistance, out int level);

            foreach (VisualEffect system in systems)
                system.gameObject.SetActive(true);

            this.currentLevel = level;
        }

        public virtual void UpdateEffect()
        {
        }

        public override void ResetEffect()
        {
            foreach (VisualEffect system in this.effectLoDs.GetAll())
            {
                system.Reinit();
                system.Stop();
                system.gameObject.SetActive(false);
            }
        }
    }
}