#region Packages

using UnityEngine;
using UnityEngine.VFX;

#endregion

namespace Runtime.VFX.Reuseable
{
    public abstract class ReuseableEffect : EffectBase
    {
        #region In

        public void PlayOne(Vector3 playerPosition, Vector3 position, Quaternion rotation)
        {
            this.effectTransform.position = position;
            this.effectTransform.rotation = rotation;

            this.Play(playerPosition);
        }

        #endregion

        #region Internal

        protected virtual void Play(Vector3 playerPosition)
        {
            float sqrDistance = (playerPosition - this.effectTransform.position).sqrMagnitude;

            foreach (VisualEffect visualEffect in this.effectLoDs.GetByDistance(sqrDistance))
            {
                visualEffect.gameObject.SetActive(true);
                visualEffect.Play();
            }
        }

        #endregion
    }
}