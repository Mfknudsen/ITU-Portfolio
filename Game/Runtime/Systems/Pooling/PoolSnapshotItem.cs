#region Libraries

using System;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.Systems.Pooling
{
    [Serializable]
    internal struct PoolSnapshotItem
    {
        [SerializeField] [Min(1)] internal int count;

        [SerializeField]
        [AssetsOnly]
        [AssetSelector(Paths = "Assets/Prefabs", Filter = "t:GameObject", IsUniqueList = false)]
        [Required]
        internal GameObject prefab;
    }
}