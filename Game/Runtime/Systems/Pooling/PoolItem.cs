#region Libraries

using UnityEngine;

#endregion

namespace Runtime.Systems.Pooling
{
    public sealed class PoolItem : MonoBehaviour
    {
        #region Values

        internal PoolHolder pool;

        #endregion

        #region Build In States

        private void OnDisable() =>
            this.pool.Free(this);

        #endregion

        #region In

        internal void Destroy() =>
            Destroy(this.gameObject);

        #endregion
    }
}