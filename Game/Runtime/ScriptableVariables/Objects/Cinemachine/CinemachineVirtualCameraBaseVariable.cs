#region Packages

using Unity.Cinemachine;
using UnityEngine;

#endregion

namespace Runtime.ScriptableVariables.Objects.Cinemachine
{
    [CreateAssetMenu(menuName = "Variables/Cinemachine/Virtual Camera")]
    public class CinemachineVirtualCameraBaseVariable : ComponentVariable<CinemachineVirtualCameraBase>
    {
        public T GetAs<T>() where T : class
        {
            return this.Value as T;
        }
    }
}