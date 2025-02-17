#region Packages

using UnityEngine;

#endregion

namespace Runtime.ScriptableVariables.Objects
{
    [CreateAssetMenu(menuName = "Variables/Camera")]
    public sealed class CameraVariable : ComponentVariable<Camera>
    {
    }
}