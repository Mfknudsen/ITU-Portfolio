#region Packages

using Runtime.Systems;
using Runtime.UI.Overworld.Sight_Alerts;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.UI.Overworld
{
    public class SightAlertUI : MonoBehaviour
    {
        [SerializeField, Required] private OperationManager operationManager;
        public static SightAlertUI instance;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this.gameObject);
        }

        #region In

        public void EnableAlert(AlertType alertType)
        {
            alertType.Trigger(this.transform, true);
            OperationsContainer container = new OperationsContainer();
            container.Add(alertType);
            this.operationManager.AddAsyncOperationsContainer(container);
        }

        public void DisableAlert(AlertType alertType)
        {
            alertType.Trigger(this.transform, false);
            OperationsContainer container = new OperationsContainer();
            container.Add(alertType);
            this.operationManager.AddAsyncOperationsContainer(container);
        }

        #endregion
    }
}