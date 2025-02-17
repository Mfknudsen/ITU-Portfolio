#region Libraries

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Runtime.Variables
{
    [Serializable]
    public sealed class SDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        #region Values

        [SerializeField, HideInInspector] private List<TKey> keyData = new List<TKey>();
        [SerializeField, HideInInspector] private List<TValue> valueData = new List<TValue>();

        #endregion

        #region Build In States

        public void OnBeforeSerialize()
        {
            this.Clear();
            for (int i = 0; i < this.keyData.Count && i < this.valueData.Count; i++)
                this[this.keyData[i]] = this.valueData[i];
        }

        public void OnAfterDeserialize()
        {
            this.keyData.Clear();
            this.valueData.Clear();

            foreach (KeyValuePair<TKey, TValue> keyValuePair in this)
            {
                this.keyData.Add(keyValuePair.Key);
                this.valueData.Add(keyValuePair.Value);
            }
        }

        #endregion
    }
}