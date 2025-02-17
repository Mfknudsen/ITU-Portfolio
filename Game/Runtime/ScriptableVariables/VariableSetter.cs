#region Packages

using UnityEngine;

#endregion

namespace Runtime.ScriptableVariables
{
    public class VariableSetter<TValue, TVariable> : MonoBehaviour
        where TValue : Object
        where TVariable : ScriptableVariable<TValue>
    {
        #region Values

        [SerializeField] private TValue value;

        [SerializeField] private TVariable variable;

        #endregion

        #region Build In States

        private void Start() => 
            this.variable.Value = this.value;

        #endregion
    }
}