#region Libraries

using System.Collections;
using Runtime.Player;
using Runtime.Player.Camera;
using Runtime.Systems.PersistantRunner;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

#endregion

namespace Runtime.Systems
{
    [CreateAssetMenu(menuName = "Manager/Camera")]
    public class CameraManager : Manager, IFrameStart
    {
        #region Values

        [SerializeField] [FoldoutGroup("Camera")] [Required]
        private PlayerManager playerManager;

        [ShowInInspector] [ReadOnly] private Camera currentCamera, defaultCamera;

        [SerializeField] [FoldoutGroup("Camera/Default")]
        private CameraEvent defaultEvent;

        private CinemachineFreeLook defaultCameraRig;
        private CinemachineVirtualCameraBase currentRig;

        #endregion

        #region Build In States

        public IEnumerator FrameStart(PersistantRunner.PersistantRunner runner)
        {
            yield return new WaitWhile(() => this.playerManager.GetOverworldCameraRig() == null);

            this.defaultCamera = Camera.main;
            this.currentCamera = this.defaultCamera;

            this.defaultCameraRig = this.playerManager.GetOverworldCameraRig();
            this.currentRig = this.defaultCameraRig;
            this.defaultCameraRig.enabled = true;

            this.ready = true;
        }

        #endregion

        #region Getters

        public Camera GetCurrentCamera()
        {
            return this.currentCamera;
        }

        public CameraEvent ReturnToDefaultOverworldEvent()
        {
            return this.defaultEvent;
        }

        #endregion

        #region In

        public void SetCurrentRigToDefault()
        {
            this.SetCurrentRig(this.defaultCameraRig);
        }

        public void SetCurrentRig(CinemachineVirtualCameraBase set)
        {
            if (set == null) return;

            if (this.currentRig != null)
                this.currentRig.Priority = 0;

            set.Priority = 1;
            this.currentRig = set;
        }

        public void SetCameraSettings(CameraSettings cameraSettings)
        {
            this.currentCamera.fieldOfView = cameraSettings.fov;
        }

        #endregion
    }
}