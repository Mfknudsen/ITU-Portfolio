using System;
using System.Collections;
using Runtime.Player;
using Runtime.Player.Camera;
using Runtime.Systems;
using Runtime.UI.Book.Light;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runtime.UI_Book
{
    [Serializable]
    internal sealed class BookOpenAction : IOperation
    {
        #region Values

        [SerializeField, Required] private UIBook uiBook;
        [SerializeField, Required] private UIBookCameraTransition transition;
        [SerializeField, Required] private UIBookLight bookLight;
        [SerializeField, Required] private OperationManager operationManager;
        [SerializeField, Required] private PlayerManager playerManager;
        [SerializeField] private CameraEvent cameraEvent;

        public bool IsOperationDone { get; private set; }

        #endregion

        public IEnumerator Operation()
        {
            this.playerManager.SetState(PlayerState.Paused);

            this.IsOperationDone = false;

            this.playerManager.DisablePlayerControl();

            OperationsContainer container = new OperationsContainer();
            this.transition.CheckMiddle();
            this.transition.Direction(false);
            
            container.Add(this.transition);
            container.Add(this.cameraEvent);

            this.operationManager.AddAsyncOperationsContainer(container);

            this.uiBook.GetVisuals().SetActive(true);

            yield return new WaitUntil(() => this.transition.IsOperationDone);

            this.uiBook.ConstructUI();
            this.IsOperationDone = true;
        }

        public void OperationEnd()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            this.bookLight.Calculate();
        }
    }
}