#region Packages

using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

#endregion

namespace Runtime.ScriptableVariables
{
    public abstract class ScriptableVariable<TGeneric> : ScriptableObject
    {
        #region Values

#if UNITY_EDITOR
        [SerializeField] protected bool debugSetter;

        // ReSharper disable once NotAccessedField.Local
        [SerializeField, TextArea] private string description;
#endif

        [SerializeField] protected TGeneric defaultValue;

        [NonSerialized, ShowInInspector, ReadOnly]
        protected TGeneric localValue;

        public TGeneric Value
        {
            get => this.localValue;
            set
            {
                if (value.Equals(this.localValue) || !this.ValueAcceptable(value)) return;

                this.localValue = value;

                this.InvokeEvents(value);

                if (this.debugSetter)
                    Debug.Log(value, this);
            }
        }

        private UnityEvent<TGeneric, TGeneric> valueChangeEventWithHistory;
        private UnityEvent<TGeneric> valueChangeEventWithValue;
        private UnityEvent valueChangeEvent;

        #endregion

        #region Build In States

        protected virtual void OnEnable() => this.localValue = this.defaultValue;

        #endregion

        #region In

        public void AddListener(UnityAction<TGeneric, TGeneric> action)
        {
            this.valueChangeEventWithHistory ??= new UnityEvent<TGeneric, TGeneric>();

            this.valueChangeEventWithHistory.AddListener(action);
        }

        public void RemoveListener(UnityAction<TGeneric, TGeneric> action) =>
            this.valueChangeEventWithHistory?.RemoveListener(action);

        public void AddListener(UnityAction<TGeneric> action)
        {
            this.valueChangeEventWithValue ??= new UnityEvent<TGeneric>();

            this.valueChangeEventWithValue.AddListener(action);
        }

        public void RemoveListener(UnityAction<TGeneric> action) =>
            this.valueChangeEventWithValue?.RemoveListener(action);

        public void AddListener(UnityAction action)
        {
            this.valueChangeEvent ??= new UnityEvent();

            this.valueChangeEvent.AddListener(action);
        }

        public void RemoveListener(UnityAction action) =>
            this.valueChangeEvent?.RemoveListener(action);

        #endregion

        #region Internal

        protected void InvokeEvents(TGeneric toCheck)
        {
            this.valueChangeEvent?.Invoke();

            this.valueChangeEventWithValue?.Invoke(toCheck);
        }

        protected virtual bool ValueAcceptable(TGeneric item) => true;

        #endregion
    }
}