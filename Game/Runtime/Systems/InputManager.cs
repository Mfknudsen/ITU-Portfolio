#region Packages

using UnityEngine;
using UnityEngine.Events;

#endregion

namespace Runtime.Systems
{
    public class InputManager
    {
        #region Values

        public static InputManager Instance => _instance ??= new InputManager();

        private static InputManager _instance;

        #region Events

        public readonly UnityEvent<Vector2>
            moveAxisInputEvent = new UnityEvent<Vector2>(),
            turnAxisInputEvent = new UnityEvent<Vector2>();

        public readonly UnityEvent
            nextChatInputEvent = new UnityEvent(),
            pauseInputEvent = new UnityEvent(),
            interactInputEvent = new UnityEvent(),
            showHideEvent = new UnityEvent(),
            jumpInputEvent = new UnityEvent();

        public readonly UnityEvent<bool>
            runInputEvent = new UnityEvent<bool>(),
            crouchInputEvent = new UnityEvent<bool>(),
            rightClickEvent = new UnityEvent<bool>(),
            leftClickEvent = new UnityEvent<bool>();

        #endregion

        #endregion

        #region Build In States

        private InputManager()
        {
            PlayerInput playerInput = new PlayerInput();

            playerInput.Player.Enable();

            playerInput.Player.MoveAxis.performed +=
                context => this.moveAxisInputEvent.Invoke(context.ReadValue<Vector2>());
            playerInput.Player.MoveAxis.canceled +=
                context => this.moveAxisInputEvent.Invoke(context.ReadValue<Vector2>());

            playerInput.Player.TurnAxis.performed +=
                context => this.turnAxisInputEvent.Invoke(context.ReadValue<Vector2>());
            playerInput.Player.TurnAxis.canceled +=
                context => this.turnAxisInputEvent.Invoke(context.ReadValue<Vector2>());

            playerInput.Player.Run.performed += _ => this.runInputEvent.Invoke(true);
            playerInput.Player.Run.canceled += _ => this.runInputEvent.Invoke(false);
            playerInput.Player.Crouch.performed += _ => this.crouchInputEvent.Invoke(true);
            playerInput.Player.Crouch.canceled += _ => this.crouchInputEvent.Invoke(false);

            playerInput.Player.NextChat.performed += _ => this.nextChatInputEvent.Invoke();
            playerInput.Player.Pause.performed += _ => this.pauseInputEvent.Invoke();
            playerInput.Player.Interact.performed += _ => this.interactInputEvent.Invoke();
            playerInput.Player.ShowHide.performed += _ => this.showHideEvent.Invoke();
            playerInput.Player.Jump.performed += _ => this.jumpInputEvent.Invoke();

            playerInput.Player.RightClick.performed += _ => this.rightClickEvent.Invoke(true);
            playerInput.Player.RightClick.canceled += _ => this.rightClickEvent.Invoke(false);

            playerInput.Player.LeftClick.performed += _ => this.leftClickEvent.Invoke(true);
            playerInput.Player.LeftClick.canceled += _ => this.leftClickEvent.Invoke(false);
        }

        #endregion

        #region Internal

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void ResetInstance() => _instance = null;

        #endregion
    }
}