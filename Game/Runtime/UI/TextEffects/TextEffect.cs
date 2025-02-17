#region Libraries

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

#endregion

namespace Runtime.UI.TextEffects
{
    public abstract class TextEffect
    {
        #region Values

        protected readonly TextEffectBase effectBase;

        private readonly UnityEvent onComplete;

        private bool isPaused;

        private Coroutine effect;

        #endregion

        #region Build In States

        protected TextEffect(TextEffectBase effectBase)
        {
            this.effectBase = effectBase;
            this.effectBase.AddPause(this.Pause);
            this.effectBase.AddForceComplete(this.Complete);
            this.effectBase.AddCancel(this.Cancel);
            this.onComplete = new UnityEvent();
            // ReSharper disable once VirtualMemberCallInConstructor
            this.effect = this.effectBase.GetText().StartCoroutine(this.Effect());
        }

        protected TextEffect(TextEffectBase effectBase, UnityAction onCompleteAction)
        {
            this.effectBase = effectBase;
            this.effectBase.AddPause(this.Pause);
            this.effectBase.AddForceComplete(this.Complete);
            this.effectBase.AddCancel(this.Cancel);
            this.onComplete = new UnityEvent();
            // ReSharper disable once VirtualMemberCallInConstructor
            this.effect = this.effectBase.GetText().StartCoroutine(this.Effect());
            this.onComplete.AddListener(onCompleteAction);
        }

        #endregion

        #region In

        public virtual void Complete()
        {
            if (this.effect != null && this.effectBase.GetText() != null)
                this.effectBase.GetText().StopCoroutine(this.effect);

            this.onComplete.Invoke();
        }

        public TextEffect AddOnComplete(UnityAction action)
        {
            this.onComplete.AddListener(action);
            return this;
        }

        public TextEffect RemoveOnComplete(UnityAction action)
        {
            this.onComplete.RemoveListener(action);
            return this;
        }

        public void Pause(bool set) => this.isPaused = set;

        public void Cancel()
        {
            this.effectBase.GetText().StopCoroutine(this.effect);
        }

        #endregion

        #region Internal

        protected abstract IEnumerator Effect();

        #endregion
    }
}