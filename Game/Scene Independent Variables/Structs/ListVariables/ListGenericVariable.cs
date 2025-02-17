#region Packages

using System.Collections.Generic;
using System.Linq;

#endregion

namespace Runtime.ScriptableVariables.Structs.ListVariables
{
    public abstract class ListGenericVariable<TGeneric> : ScriptableVariable<List<TGeneric>>
    {
        #region Build In States

        protected override void OnEnable() =>
            this.localValue = this.defaultValue.ToList();

        #endregion

        #region In

        public void AddElement(TGeneric element, bool allowMultiplySameInstances = false)
        {
            this.Value ??= new List<TGeneric>();

            if (!allowMultiplySameInstances && this.Value.Contains(element)) return;

            this.Value.Add(element);
            this.InvokeEvents(this.Value);
        }

        public void RemoveElement(TGeneric element)
        {
            this.Value ??= new List<TGeneric>();

            this.Value.Remove(element);
            this.InvokeEvents(this.Value);
        }

        #endregion
    }
}