#region Libraries

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#endregion

namespace Runtime.Systems.Pooling
{
    [InitializeOnLoad]
    public static class Pool
    {
        #region Values

        private static readonly Dictionary<int, PoolHolder> Pools = new Dictionary<int, PoolHolder>();

        #endregion

        #region Build In States

        static Pool()
        {
            EditorApplication.playModeStateChanged += InPlayModeExit;
        }

        #endregion

        #region In

        public static void AddSnapshot(int id, Object prefab, int count)
        {
            if (Pools.TryGetValue(prefab.GetHashCode(), out PoolHolder pool))
                pool.AddSnapCount(id, count);
        }

        public static void RemoveSnapshot(int id, Object prefab)
        {
            if (Pools.TryGetValue(prefab.GetHashCode(), out PoolHolder pool))
                pool.RemoveSnapCount(id);
        }

        public static GameObject Create(Object prefab, Transform parent = null, bool activate = false)
        {
            return CreatePoolItem(prefab, activate, parent);
        }

        public static GameObject CreateAtPositionAndRotation(Object prefab, Vector3 position, Quaternion rotation,
            Transform parent = null, bool activate = false)
        {
            GameObject instance = CreatePoolItem(prefab, activate, parent);
            Transform t = instance.transform;
            t.position = position;
            t.rotation = rotation;
            return instance;
        }

        public static GameObject CreateAtTransform(Object prefab, Transform sourceTransform,
            Transform parent = null, bool activate = false)
        {
            GameObject instance = CreatePoolItem(prefab, activate, parent);
            Transform t = instance.transform;
            t.position = sourceTransform.position;
            t.rotation = sourceTransform.rotation;
            return instance;
        }

        public static GameObject CreateAsChild(Object prefab, Transform parent, bool activate = false)
        {
            GameObject instance = CreatePoolItem(prefab, activate, parent);
            Transform t = instance.transform;
            t.parent = parent;
            t.position = parent.position;
            t.rotation = parent.rotation;
            return instance;
        }

        #endregion

        #region Internal

        private static GameObject CreatePoolItem(Object prefab, bool activateObject, Transform parent = null)
        {
            if (prefab == null)
                return null;

            int key = prefab.GetHashCode();
            if (!Pools.TryGetValue(key, out PoolHolder pool))
            {
                pool = new PoolHolder(prefab);
                Pools.Add(key, pool);
            }

            Object instance = pool.GetOrCreate();

            GameObject obj = instance switch
            {
                GameObject gameObject => gameObject,
                MonoBehaviour monoBehaviour => monoBehaviour.gameObject,
                _ => null
            };

            if (obj == null)
                return null;

            if (parent != null)
                obj.transform.SetParent(parent, false);

            obj.SetActive(activateObject);

            return obj;
        }

#if UNITY_EDITOR
        private static void InPlayModeExit(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode)
                return;

            Reset();
        }

        private static void Reset()
        {
            Pools.Clear();
        }
#endif

        #endregion

        #region Tests

#if UNITY_INCLUDE_TESTS

        public static void ResetForTests()
        {
            Reset();
        }

        public static void InitializeForTests()
        {
        }

        public static bool GetHolder(int key, out PoolHolder result)
        {
            return Pools.TryGetValue(key, out result);
        }
#endif

        #endregion
    }
}