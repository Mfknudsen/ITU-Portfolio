#region Packages

using Runtime.Core;
using Runtime.ScriptableVariables.Objects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.VFX;

#endregion

namespace Runtime.VFX.Scene.Foliage
{
    public sealed class FallingLeafsEffect : SceneEffect
    {
        #region Values

        [SerializeField, Required] private TransformGenericVariable playerTransformGeneric;
        [SerializeField] private float maxDistance;
        
        #endregion

        #region Internal

        protected override void Play()
        {
            float sqrDistance = (this.playerTransformGeneric.Position - this.effectTransform.position).sqrMagnitude;
            VisualEffect[] visualEffects = this.effectLoDs.GetByDistance(sqrDistance, out int level);

            foreach (VisualEffect visualEffect in visualEffects)
                visualEffect.Play();

            this.currentLevel = level;

            this.isSwitching = false;
        }

        protected override void Stop()
        {
            foreach (VisualEffect visualEffect in this.effectLoDs.GetByID(this.currentLevel))
                visualEffect.Stop();

            this.isSwitching = false;
        }

        protected override bool Rules() =>
            this.effectTransform.position.QuickDistanceLessThen(this.playerTransformGeneric.Value.position, this.maxDistance);

        protected override void OnStart()
        {
            foreach (VisualEffect visualEffect in this.effectLoDs.GetAll())
            {
                visualEffect.Reinit();
                visualEffect.Stop();
            }
        }

        #endregion
    }
}