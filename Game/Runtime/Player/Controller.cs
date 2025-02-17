#region Libraries

using System.Collections;
using Runtime.Core;
using Runtime.ScriptableVariables.Objects.Cinemachine;
using Runtime.ScriptableVariables.Structs;
using Runtime.Systems;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

#endregion

namespace Runtime.Player
{
    public sealed class Controller : MonoBehaviour
    {
        #region Values

        [SerializeField] [FoldoutGroup("Components")]
        private NavMeshAgent agent;

        [SerializeField] [FoldoutGroup("Components")]
        private Rigidbody rb;

        [SerializeField] [FoldoutGroup("Components")]
        private Animator animController;

        [SerializeField] [FoldoutGroup("Components")]
        private CinemachineFreeLook cameraRig;

        [SerializeField] [FoldoutGroup("Transforms")]
        private Transform playerTransform,
            moveTransform,
            visualTransform;

        [SerializeField] [FoldoutGroup("Speeds")]
        private float moveSpeed,
            rotateSpeed,
            runSpeed;

        [SerializeField] [BoxGroup("Speeds/Dash")]
        private float dashTime;

        [SerializeField] [BoxGroup("Speeds/Dash")]
        private AnimationCurve dashCurve;

        [SerializeField] [HorizontalGroup("Speeds/Camera")]
        private float yCamSpeed,
            xCamSpeed;

        [SerializeField] [FoldoutGroup("Animation")]
        private float animatorDamp = 0.1f;

        [SerializeField] [FoldoutGroup("Variables")] [Required]
        private PlayerManager playerManager;

        [SerializeField] [FoldoutGroup("Variables")] [Required]
        private BoolGenericVariable aiming,
            allowed,
            running;

        [SerializeField] private Vector2 moveDirection;

        [SerializeField] [FoldoutGroup("Variables")] [Required]
        private CinemachineBrainVariable cameraBrain;

        private bool ready,
            isRunning,
            isCrouching;

        private Vector3 toLookRotation = Vector3.forward;

        private static readonly int HashWalking = Animator.StringToHash("WalkSpeed"),
            HashCrouch = Animator.StringToHash("Crouch"),
            HashDash = Animator.StringToHash("Dash");

        #endregion

        #region Build In States

        private void Start()
        {
            this.playerTransform = this.transform;

            this.agent ??= this.playerTransform.GetComponent<NavMeshAgent>();
            this.rb ??= this.playerTransform.GetComponent<Rigidbody>();
            this.rb.useGravity = false;
        }

        private void Update()
        {
            Debug.DrawRay(this.moveTransform.position, this.moveTransform.forward * 2, Color.blue);

            if (!this.allowed.Value) return;

            this.UpdateMoveTransform();
            this.Move();
            this.Turn();
            this.UpdateAnimController();
        }

        #endregion

        #region In

        public void Enable()
        {
            InputManager inputManager = InputManager.Instance;
            inputManager.moveAxisInputEvent.AddListener(this.OnMoveDirectionChange);
            inputManager.crouchInputEvent.AddListener(this.OnCrouchChange);
            inputManager.jumpInputEvent.AddListener(this.OnJumpChange);
            inputManager.crouchInputEvent.AddListener(this.OnCrouchChange);
            inputManager.runInputEvent.AddListener(this.OnRunChange);

            this.allowed.Value = true;

            this.cameraRig.m_YAxis.m_MaxSpeed = this.yCamSpeed;
            this.cameraRig.m_XAxis.m_MaxSpeed = this.xCamSpeed;
        }

        public void Disable()
        {
            InputManager inputManager = InputManager.Instance;
            inputManager.moveAxisInputEvent.RemoveListener(this.OnMoveDirectionChange);
            inputManager.crouchInputEvent.RemoveListener(this.OnCrouchChange);
            inputManager.jumpInputEvent.RemoveListener(this.OnJumpChange);
            inputManager.crouchInputEvent.RemoveListener(this.OnCrouchChange);
            inputManager.runInputEvent.RemoveListener(this.OnRunChange);

            this.allowed.Value = false;

            this.cameraRig.m_YAxis.m_MaxSpeed = 0;
            this.cameraRig.m_XAxis.m_MaxSpeed = 0;
        }

        public void TriggerAnimator(int triggerID)
        {
            this.animController.SetTrigger(triggerID);
        }

        #endregion

        #region Internal

        private void UpdateMoveTransform()
        {
            if (this.aiming.Value) return;

            Vector3 camForward = this.cameraBrain.getTransform.forward;
            camForward -= Vector3.up * camForward.y;

            this.moveTransform.LookAt(this.moveTransform.position + camForward, Vector3.up);
        }

        private void UpdateAnimController()
        {
            if (!this.animController) return;

            this.animController.SetFloat(
                HashWalking,
                this.moveDirection.magnitude * 2 * (this.isRunning ? 3 : 1),
                this.animatorDamp,
                Time.deltaTime);
        }

        private void Move()
        {
            if (!this.agent ||
                !this.agent.isOnNavMesh ||
                this.playerManager.GetPlayerState() == PlayerState.Dashing) return;

            Vector2 playerInputDirection = this.moveDirection;
            Vector3 forwardMove = this.moveTransform.forward * playerInputDirection.y;
            Vector3 sideMove = this.moveTransform.right * playerInputDirection.x;
            Vector3 moveVector = (forwardMove + sideMove).normalized;

            this.agent.Move(moveVector.normalized *
                            ((this.running.Value ? this.runSpeed : this.moveSpeed) * Time.deltaTime));
        }

        private void Turn()
        {
            if (!this.aiming.Value)
            {
                if (this.moveDirection != Vector2.zero)
                {
                    Vector2 playerInputDirection = this.moveDirection;
                    Vector3 forwardMove = this.moveTransform.forward * playerInputDirection.y;
                    Vector3 sideMove = this.moveTransform.right * playerInputDirection.x;
                    this.toLookRotation = (forwardMove + sideMove).normalized;
                    this.toLookRotation -= new Vector3(0, this.toLookRotation.y, 0);
                }

                this.visualTransform.rotation = Quaternion.Lerp(this.visualTransform.rotation,
                    Quaternion.LookRotation(this.toLookRotation), this.rotateSpeed * Time.deltaTime);
            }
            else
            {
                Vector3 camForward = this.cameraBrain.getTransform.forward;
                camForward -= Vector3.up * camForward.y;
                this.toLookRotation = camForward;
            }
        }

        private void TryDash()
        {
            PlayerState state = this.playerManager.GetPlayerState();

            if (state is not (PlayerState.Default or PlayerState.Crouching)) return;

            this.playerManager.SetState(PlayerState.Dashing);

            this.animController.SetTrigger(HashDash);
            this.StartCoroutine(this.Dash());
        }

        private IEnumerator Dash()
        {
            float timeLeft = this.dashTime;
            Vector3 forward = this.moveTransform.forward;
            Vector3 direction = new Vector3(this.moveDirection.x, 0, this.moveDirection.y);

            if (this.moveDirection == Vector2.zero)
                direction = forward;

            while (timeLeft > 0)
            {
                if (this.moveDirection != Vector2.zero)
                    direction = new Vector3(this.moveDirection.x, 0, this.moveDirection.y);

                this.agent.Move((this.moveTransform.forward * direction.z +
                                 this.moveTransform.right * direction.x).normalized *
                                this.dashCurve.Evaluate(timeLeft.PercentageOf(this.dashTime) / 100f));

                timeLeft -= Time.deltaTime;
                yield return null;
            }

            this.playerManager.SetState(this.isCrouching ? PlayerState.Crouching : PlayerState.Default);
        }

        private void OnMoveDirectionChange(Vector2 input)
        {
            this.moveDirection = input;
        }

        private void OnCrouchChange(bool input)
        {
            if (this.isCrouching != input)
                this.animController.SetBool(HashCrouch, input);

            this.isCrouching = input;
        }

        private void OnRunChange(bool input)
        {
            this.running.Value = input;
        }

        private void OnJumpChange()
        {
            this.TryDash();
        }

        #endregion
    }
}