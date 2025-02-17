#region Libraries

using Unity.Cinemachine;
using UnityEngine;

#endregion

namespace Runtime.ScriptableVariables.Objects.Cinemachine
{
    [CreateAssetMenu(menuName = "Variables/CinemachineBrain")]
    public sealed class CinemachineBrainVariable : ComponentVariable<CinemachineBrain>
    {
    }
}