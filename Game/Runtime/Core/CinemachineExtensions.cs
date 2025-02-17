#region Libraries

using Unity.Cinemachine;

#endregion

namespace Runtime.Core
{
    public static class CinemachineExtensions
    {
        public static TComponent CinemachineComponent<TComponent>(this CinemachineVirtualCameraBase virtualCamera)
            where TComponent : CinemachineComponentBase
        {
            return (virtualCamera as CinemachineVirtualCamera)?.GetCinemachineComponent<TComponent>();
        }
    }
}