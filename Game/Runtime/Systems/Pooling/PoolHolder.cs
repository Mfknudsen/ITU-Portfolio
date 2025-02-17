#region Libraries

using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

#endregion

namespace Runtime.Systems.Pooling
{
    public struct PoolHolder
    {
        #region Values

        private readonly Object prefab;

        private readonly Stack<PoolItem> freeObjects;
        private readonly HashSet<int> usedObjects;
        private readonly Dictionary<int, int> snapCount;

        private readonly List<PoolMinimum> poolMinimums;

        private int currentMinimum;

        #endregion

        #region Build In States

        public PoolHolder(Object prefab)
        {
            this.prefab = prefab;

            this.freeObjects = new Stack<PoolItem>();
            this.usedObjects = new HashSet<int>();
            this.snapCount = new Dictionary<int, int>();
            this.poolMinimums = new List<PoolMinimum>();

            this.currentMinimum = 1;
        }

        #endregion

        #region Getters

        private int TotalObjects =>
            this.freeObjects.Count + this.usedObjects.Count;

        #endregion

        #region In

        public void AddMinimum(PoolMinimum minimum)
        {
            if (this.poolMinimums.Contains(minimum))
                return;

            this.poolMinimums.Add(minimum);

            if (this.currentMinimum < minimum.GetCount())
                this.currentMinimum = minimum.GetCount();
        }

        public void RemoveMinimum(PoolMinimum minimum)
        {
            if (!this.poolMinimums.Contains(minimum))
                return;

            this.poolMinimums.Remove(minimum);

            if (this.currentMinimum != minimum.GetCount()) return;

            int count = 1;
            foreach (PoolMinimum poolMinimum in this.poolMinimums)
            {
                if (poolMinimum.GetCount() == this.currentMinimum)
                {
                    count = this.currentMinimum;
                    break;
                }

                if (poolMinimum.GetCount() > count)
                    count = poolMinimum.GetCount();
            }

            this.currentMinimum = count;
        }

        public void AddSnapCount(int hash, int count)
        {
            this.snapCount.TryAdd(hash, count);

            int min = this.currentMinimum;
            this.snapCount.Values.ForEach(v =>
            {
                if (v > min)
                    min = v;
            });

            this.CheckMinimumInstances();
        }

        public void RemoveSnapCount(int hash)
        {
            this.snapCount.Remove(hash);
        }

        #endregion

        #region Out

        // ReSharper disable Unity.PerformanceAnalysis
        public Object GetOrCreate()
        {
            Object instance;

            if (this.freeObjects.Count == 0)
            {
                instance = this.Create();
#if UNITY_EDITOR
                if (instance == null)
                {
                    Debug.LogError("Failed to create instance for: " + this.prefab.name);
                    return null;
                }
#endif
                PoolItem item = instance switch
                {
                    GameObject gameObject => gameObject.AddComponent<PoolItem>(),
                    MonoBehaviour monoBehaviour => monoBehaviour.gameObject.AddComponent<PoolItem>(),
                    _ => null
                };

                if (item == null)
                {
                    Debug.LogError($"Failed to create pool item for {instance.name}");

                    return instance;
                }

                item.pool = this;
            }
            else
            {
                instance = this.freeObjects.Pop().gameObject;
            }

            this.usedObjects.Add(instance.GetInstanceID());
            return instance;
        }

        public void Free(PoolItem instance)
        {
            if (this.usedObjects.Remove(instance.GetInstanceID()))
                this.freeObjects.Push(instance);

            int diff = this.TotalObjects - this.currentMinimum;

            if (diff <= 0) return;

            List<PoolItem> list = new List<PoolItem>();
            for (int i = 0; i < diff; i++)
            {
                if (this.freeObjects.Count == 0)
                    break;

                list.Add(this.freeObjects.Pop());
            }

            for (int i = list.Count - 1; i >= 0; i--)
                list[i].Destroy();
        }

        #endregion

        #region Internal

        private Object Create()
        {
            return Object.Instantiate(this.prefab);
        }

        private void CheckMinimumInstances()
        {
            int totalCurrent = this.TotalObjects;

            if (this.currentMinimum - totalCurrent <= 0)
                return;

            for (int i = 0; i < this.currentMinimum - totalCurrent; i++)
            {
                Object instance = this.Create();

                if (instance is GameObject gameObject)
                    this.freeObjects.Push(gameObject.GetComponent<PoolItem>());
                else if (instance is MonoBehaviour monoBehaviour)
                    this.freeObjects.Push(monoBehaviour.gameObject.GetComponent<PoolItem>());
            }
        }

        #endregion

        #region Tests

        public bool MatchPrefab(Object toMatch)
        {
            return toMatch.Equals(this.prefab);
        }

        #endregion
    }
}