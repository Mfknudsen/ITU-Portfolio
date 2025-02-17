#region Packages

using Runtime.Systems;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.VFX
{
    public abstract class EffectBase : MonoBehaviour
    {
        #region Values

        [SerializeField, Required] private EffectManager effectManager;

        [ShowInInspector, ReadOnly] protected int currentLevel = -1;

        [SerializeField] protected EffectLOD effectLoDs;

        protected Transform effectTransform;

        #endregion

        #region Build In States

        private void OnValidate() => this.effectLoDs.Validate();

        private void Start() => this.effectTransform = this.transform;

        private void OnEnable()
        {
            this.effectManager.RegisterEffect(this);

            this.OnStart();
        }

        private void OnDisable()
        {
            this.effectManager.UnregisterEffect(this);

            this.OnEnd();
        }

        #endregion

        #region In

        public virtual void ResetEffect()
        {
        }

        #endregion

        #region Internal

        protected virtual void OnStart()
        {
        }

        protected virtual void OnEnd()
        {

        }

        #endregion
    }
}