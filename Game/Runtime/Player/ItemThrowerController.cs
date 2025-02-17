#region Libraries

using Runtime.Core;
using Runtime.Player.Camera;
using Runtime.ScriptableVariables.Objects.Cinemachine;
using Runtime.ScriptableVariables.Objects.Items;
using Runtime.ScriptableVariables.Structs;
using Runtime.Systems;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

#endregion

namespace Runtime.Player
{
    public class ItemThrowerController : MonoBehaviour
    {
        #region Values

        [SerializeField] [Required] private OperationManager operationManager;

        [SerializeField] [Required] private CinemachineComponentBase cameraRig;
        [SerializeField] [Required] private BoolGenericVariable aiming, throwing, allowed;
        [SerializeField] [Required] private ItemVariable toThrow;
        [SerializeField] [Required] private CinemachineVirtualCameraBaseVariable defaultOverworldRig;
        [SerializeField] [Required] private CinemachineBrainVariable cameraBrain;
        [SerializeField] [Required] private Vec2GenericVariable throwRotationSpeed;
        [SerializeField] [Required] private Transform throwTransform, moveTransform;

        //Cam
        [SerializeField] private CameraEvent intoThrowCameraEvent, outThrowCameraEvent;

        [SerializeField] private float shoulderOffset;
        [SerializeField] private Vector2 top, mid, bot;
        [SerializeField] private AnimationCurve lerpCurve;

        [SerializeField] private float current;

        private CinemachineThirdPersonFollow bodyComponent;
        private CinemachineComposer aimComponent;

        private OperationsContainer cameraSwitchAsyncContainer;

        private float currentShoulderOffset;

        private IOperation throwingOperation;

        private Timer throwDelay;

        #endregion

        #region Build In States

        private void OnValidate()
        {
            this.bodyComponent = this.cameraRig.GetComponent<CinemachineThirdPersonFollow>();
            this.aimComponent = this.cameraRig.GetComponent<CinemachineComposer>();

            this.RotateCameraY();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            Transform t = this.transform;
            for (float i = 0.05f; i <= 1; i += .05f)
            {
                float l = this.lerpCurve.Evaluate(i);
                float j = this.lerpCurve.Evaluate(i - .05f);

                Gizmos.DrawLine(
                    t.position
                    + Vector3.up * .4f + Vector3.right * this.shoulderOffset
                    - t.forward * (i < .5f
                        ? Mathf.Lerp(this.bot.y, this.mid.y, l)
                        : Mathf.Lerp(this.mid.y, this.top.y, 1f - l))
                    + Vector3.up * (i < .5f
                        ? Mathf.Lerp(this.bot.x, this.mid.x, i * 2f)
                        : Mathf.Lerp(this.mid.x, this.top.x, (i - .5f) * 2f)),
                    t.position
                    + Vector3.up * .4f + Vector3.right * this.shoulderOffset
                    - t.forward * (i - .05f < .5f
                        ? Mathf.Lerp(this.bot.y, this.mid.y, j)
                        : Mathf.Lerp(this.mid.y, this.top.y, 1f - j))
                    + Vector3.up * (i - .05f < .5f
                        ? Mathf.Lerp(this.bot.x, this.mid.x, (i - .05f) * 2f)
                        : Mathf.Lerp(this.mid.x, this.top.x, (i - .05f - .5f) * 2f)));
            }
        }

        private void OnEnable()
        {
            InputManager inputManager = InputManager.Instance;
            inputManager.rightClickEvent.AddListener(input => this.aiming.Value = input);
            inputManager.leftClickEvent.AddListener(_ => this.ThrowItem());
            inputManager.turnAxisInputEvent.AddListener(this.RotateCamera);

            this.aiming.AddListener(this.SwitchToAiming);

            this.bodyComponent = this.cameraRig.GetComponent<CinemachineThirdPersonFollow>();
            this.aimComponent = this.cameraRig.GetComponent<CinemachineComposer>();
        }

        private void OnDisable()
        {
            InputManager inputManager = InputManager.Instance;
            inputManager.rightClickEvent.RemoveListener(input => this.aiming.Value = input);
            inputManager.leftClickEvent.RemoveListener(_ => this.ThrowItem());
            inputManager.turnAxisInputEvent.RemoveListener(this.RotateCamera);

            this.aiming.RemoveListener(this.SwitchToAiming);

            this.throwDelay?.Stop();
        }

        #endregion

        #region Internal

        private void RotateCamera(Vector2 input)
        {
            this.RotateCameraX(input.x);
            this.RotateCameraY(input.y);
        }

        private void RotateCameraY(float input = 0)
        {
            if (input != 0)
                this.current -= input * this.throwRotationSpeed.y * Time.deltaTime;
            this.current.RefClamp(0f, 1f);

            if (this.bodyComponent == null || this.aimComponent == null) return;

            float lerpCurrent = this.lerpCurve.Evaluate(this.current);

            this.bodyComponent.CameraDistance = this.current < .5f
                ? Mathf.Lerp(this.bot.y, this.mid.y, lerpCurrent)
                : Mathf.Lerp(this.mid.y, this.top.y, 1f - lerpCurrent);

            this.bodyComponent.ShoulderOffset = new Vector3(
                this.shoulderOffset,
                this.current < .5f
                    ? Mathf.Lerp(this.bot.x, this.mid.x, this.current * 2f)
                    : Mathf.Lerp(this.mid.x, this.top.x, (this.current - .5f) * 2f),
                0f);

            this.aimComponent.m_TrackedObjectOffset = Vector3.right * this.shoulderOffset;
        }

        private void RotateCameraX(float input)
        {
            this.moveTransform.Rotate(Vector3.up, input * Time.deltaTime);
        }

        private void ThrowItem()
        {
            if (!this.allowed.Value || this.toThrow.valueEmpty || this.throwing.Value || !this.aiming.Value) return;

            this.throwing.Value = true;

            GameObject spawnedItem = Instantiate(
                this.toThrow.GetVisual(),
                this.throwTransform.position + this.cameraBrain.getTransform.forward * .5f,
                this.cameraBrain.getTransform.rotation);

            spawnedItem.GetComponent<Rigidbody>()
                .AddForce(this.cameraBrain.getTransform.forward * 10, ForceMode.Impulse);

            this.throwDelay = new Timer(1, () =>
            {
                this.throwing.Value = false;
                this.throwDelay = null;
            });

            Debug.Log("Spawned");
        }

        private void SwitchToAiming()
        {
            if (!this.allowed.Value) return;

            if (this.cameraSwitchAsyncContainer != null)
                this.operationManager.StopAsyncContainer(this.cameraSwitchAsyncContainer);

            this.cameraSwitchAsyncContainer = new OperationsContainer();

            CameraEvent selectedCameraEvent;

            if (this.aiming.Value)
            {
                this.bodyComponent.VirtualCamera.Follow = this.moveTransform;

                this.current = this.defaultOverworldRig.GetAs<CinemachineFreeLook>().m_YAxis.Value;
                this.RotateCameraY();

                selectedCameraEvent = this.intoThrowCameraEvent;
            }
            else
            {
                this.bodyComponent.VirtualCamera.Follow = null;

                this.defaultOverworldRig.GetAs<CinemachineFreeLook>().m_YAxis.Value = this.current;
                this.defaultOverworldRig.GetAs<CinemachineFreeLook>().m_XAxis.Value =
                    this.cameraBrain.getTransform.rotation.eulerAngles.y;

                selectedCameraEvent = this.outThrowCameraEvent;
            }

            this.cameraSwitchAsyncContainer.Add(selectedCameraEvent);
            this.operationManager.AddAsyncOperationsContainer(this.cameraSwitchAsyncContainer);
        }

        #endregion
    }
}