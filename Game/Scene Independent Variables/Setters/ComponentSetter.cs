#region Packages

using UnityEngine;

#endregion

namespace Runtime.ScriptableVariables.Setters
{
    public class ComponentSetter<TComponent, TComponentVariable> : VariableSetter<TComponent, TComponentVariable>
        where TComponent : Component where TComponentVariable : ComponentVariable<TComponent>
    {
    }
}