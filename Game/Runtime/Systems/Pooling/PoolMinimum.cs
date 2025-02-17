#region Libraries

using System;
using UnityEngine;

#endregion

namespace Runtime.Systems.Pooling
{
    [Serializable]
    public struct PoolMinimum
    {
        #region Values

        [SerializeField] [Min(1)] private int count;

        #endregion

        #region Getters

        public int GetCount()
        {
            return this.count;
        }

        #endregion
    }
}