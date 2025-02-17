#region Packages

using Runtime.World.Overworld.Interactions;
using UnityEngine;

#endregion

namespace Runtime.ScriptableVariables.Structs.ListVariables
{
    [CreateAssetMenu(menuName = "Variables/List/Interact Item")]
    public class InteractItemListGenericVariable : ListGenericVariable<InteractItem>
    {
    }
}