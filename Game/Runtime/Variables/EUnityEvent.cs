#region Libraries

using UnityEngine.Events;

#endregion

namespace Runtime.Variables
{
    public sealed class EUnityEvent<T> : UnityEvent<T>
    {
        #region Values

        private int addedCount;

        #endregion

        #region Getters

        public int AddedCount() => this.addedCount;

        #endregion

        #region In

        public new void AddListener(UnityAction<T> action)
        {
            if (action == null)
                return;

            this.addedCount++;

            base.AddListener(action);
        }

        public new void RemoveListener(UnityAction<T> action)
        {
            if (action == null)
                return;

            this.addedCount--;

            base.RemoveListener(action);
        }

        #endregion
    }
}