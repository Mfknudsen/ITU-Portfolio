#region Packages

using System;
using System.Collections;
using Runtime.Player;
using Runtime.Player.Camera;
using Runtime.Systems;
using Runtime.Systems.UI;
using Runtime.UI.Book.Light;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.UI_Book
{
    [Serializable]
    internal sealed class CloseBookAction : IOperation
    {
        #region Values

        [SerializeField, Required] private UIBook uiBook;
        [SerializeField, Required] private UIBookCameraTransition transition;
        [SerializeField, Required] private UIBookLight bookLight;
        [SerializeField, Required] private OperationManager operationManager;
        [SerializeField, Required] private UIManager uiManager;
        [SerializeField, Required] private PlayerManager playerManager;
        [SerializeField] private CameraEvent cameraEvent;

        public bool IsOperationDone { get; private set; }

        #endregion

        public IEnumerator Operation()
        {
            this.IsOperationDone = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            this.bookLight.TurnOff();

            OperationsContainer container = new OperationsContainer();
            this.transition.Direction(true);
            container.Add(this.transition);
            container.Add(this.cameraEvent);
            this.operationManager.AddAsyncOperationsContainer(container);

            yield return new WaitUntil(() => this.transition.IsOperationDone);

            this.uiManager.SwitchUI(UISelection.Overworld);

            this.IsOperationDone = true;
        }

        public void OperationEnd()
        {
            this.uiBook.GetVisuals().SetActive(false);
            this.uiManager.SetReadyToPause(true);
            this.playerManager.EnablePlayerControl();

            this.playerManager.SetState(PlayerState.Default);
        }
    }
}