#region Packages

using Runtime.ScriptableVariables.Structs;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.Player.Camera
{
    public sealed class ThrowCameraRigController : MonoBehaviour
    {
        #region Values

        [SerializeField, Required] private Transform followObject, visualObject;

        [SerializeField, Required] private BoolGenericVariable allowed, aiming;

        [SerializeField, Required] private Vec2GenericVariable rotationDirection, rotationSpeeds;

        private Transform controllerTransform;

        #endregion

        #region Build In States

        private void Awake()
        {
            this.controllerTransform = this.transform;
        }

        private void Update()
        {
            if (!this.allowed.Value) return;

            this.controllerTransform.position = this.followObject.position;

            if (!this.aiming.Value) return;
            
            this.followObject.Rotate(Vector3.up, this.rotationDirection.x * this.rotationSpeeds.x * Time.deltaTime);

            Quaternion followRotation = this.followObject.rotation;

            this.controllerTransform.rotation = followRotation;

            this.visualObject.rotation = Quaternion.Lerp(this.visualObject.rotation,
                followRotation, 100 * Time.deltaTime);
        }

        #endregion
    }
}