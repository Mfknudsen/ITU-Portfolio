#region Packages

using UnityEngine;
using UnityEngine.Events;

#endregion

namespace Runtime.ScriptableEvents
{
    public interface IScriptableEvent<T>
    {
        public void AddListener(UnityAction<T> action);

        public void RemoveListener(UnityAction<T> action);

        public void Trigger(T value);
    }

    public interface IScriptableEvent<T1, T2>
    {
        public void AddListener(UnityAction<T1, T2> action);

        public void RemoveListener(UnityAction<T1, T2> action);

        public void Trigger(T1 value1, T2 value2);
    }

    public interface IScriptableEvent<T1, T2, T3>
    {
        public void AddListener(UnityAction<T1, T2, T3> action);

        public void RemoveListener(UnityAction<T1, T2, T3> action);

        public void Trigger(T1 value1, T2 value2, T3 value3);
    }
}