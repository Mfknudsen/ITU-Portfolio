#region Packages

using UnityEngine;

#endregion

namespace Runtime.ScriptableVariables
{
    public class ObjectVariable<TGeneric> : ScriptableVariable<TGeneric> where TGeneric : Object
    {
        public bool valueEmpty => this.Value == null;
    }
}