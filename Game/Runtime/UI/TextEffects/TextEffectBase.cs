#region Libraries

using TMPro;
using UnityEngine.Events;

#endregion

namespace Runtime.UI.TextEffects
{
    public class TextEffectBase
    {
        #region Values

        private readonly TextMeshProUGUI text;

        private readonly UnityEvent onForceComplete, onCancel;
        private readonly UnityEvent<bool> onPause;

        #endregion

        #region Build In States

        public TextEffectBase(TextMeshProUGUI text)
        {
            this.text = text;
            this.onForceComplete = new UnityEvent();
            this.onCancel = new UnityEvent();
            this.onPause = new UnityEvent<bool>();
        }

        #endregion

        #region Getters

        internal TextMeshProUGUI GetText() => this.text;

        #endregion

        #region In

        public void ForceComplete() =>
            this.onForceComplete.Invoke();

        public void Cancel() =>
            this.onCancel.Invoke();

        public void Pause(bool isPause) =>
            this.onPause.Invoke(isPause);

        internal void AddForceComplete(UnityAction action) =>
            this.onForceComplete.AddListener(action);

        internal void RemoveForceComplete(UnityAction action) =>
            this.onForceComplete.RemoveListener(action);

        internal void AddCancel(UnityAction action) =>
            this.onCancel.AddListener(action);

        internal void RemoveCancel(UnityAction action) =>
            this.onCancel.RemoveListener(action);

        internal void AddPause(UnityAction<bool> action) =>
            this.onPause.AddListener(action);

        internal void RemovePause(UnityAction<bool> action) =>
            this.onPause.RemoveListener(action);

        #endregion
    }
}