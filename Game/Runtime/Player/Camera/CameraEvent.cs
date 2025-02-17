#region Libraries

using System;
using System.Collections;
using Runtime.Systems;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

#endregion

namespace Runtime.Player.Camera
{
    [Serializable]
    public class CameraEvent : IOperation
    {
        #region Values

        [SerializeField] [TextArea] private string description;

        [SerializeField] private bool useDefaultRig;

        [SerializeField] [Required] private CameraManager cameraManager;

        [SerializeField] [Required] [HideIf("useDefaultRig")]
        private CinemachineVirtualCameraBase cinemachineRig;

        [SerializeField] private CameraSettings cameraSettings;

        [SerializeField] [Range(0, 1)] private float percentToEnable;
        [SerializeField] private float timeInSeconds;

        private bool done;

        #endregion

        #region Getters

        public float GetTime => this.timeInSeconds;

        #endregion

        #region In

        public bool IsOperationDone => this.done;

        public IEnumerator Operation()
        {
            this.done = false;

            yield return new WaitForSeconds(this.timeInSeconds * this.percentToEnable);

            if (!this.useDefaultRig)
                this.cameraManager.SetCurrentRig(this.cinemachineRig);
            else
                this.cameraManager.SetCurrentRigToDefault();

            if (this.cameraSettings != null) this.cameraManager.SetCameraSettings(this.cameraSettings);

            yield return new WaitForSeconds(this.timeInSeconds * (1 - this.percentToEnable));

            this.done = true;
        }

        public void OperationEnd()
        {
        }

        #endregion
    }
}