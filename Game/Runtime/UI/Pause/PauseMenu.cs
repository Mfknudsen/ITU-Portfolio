#region Packages

using JetBrains.Annotations;
using Runtime.Systems.UI;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.UI.Pause
{
    public class PauseMenu : MonoBehaviour
    {
        #region Values

        [SerializeField, Required] private UIManager uiManager;
        [SerializeField] private GameObject defaultPage, optionsPage, savePage, exitPage;

        private UISelection preSelection;
        private bool showMouse;

        #endregion

        #region In

        public void OnDisplay(UISelection toReturnSelection)
        {
            //Cursor.visible = !showMouse;
            //Cursor.lockState = showMouse ? CursorLockMode.Confined : CursorLockMode.None;

            this.showMouse = !this.showMouse;
            
            if (toReturnSelection != UISelection.Pause) this.preSelection = toReturnSelection;

            this.defaultPage.SetActive(true);
            this.optionsPage.SetActive(false);
            this.savePage.SetActive(false);
            this.exitPage.SetActive(false);
        }

        #region Navigation

        [UsedImplicitly]
        public void Unpause()
        {
            this.uiManager.SwitchUI(this.preSelection);
        }

        [UsedImplicitly]
        public void ActivateOptions(bool toActivate)
        {
            this.defaultPage.SetActive(!toActivate);
            this.optionsPage.SetActive(toActivate);

            this.optionsPage.GetComponent<Options>().Gameplay();
        }


        [UsedImplicitly]
        public void ActivateSave(bool toActivate)
        {
            this.defaultPage.SetActive(!toActivate);
            this.savePage.SetActive(toActivate);
        }


        [UsedImplicitly]
        public void Exit()
        {
            this.exitPage.SetActive(true);
        }

        #endregion

        #endregion
    }
}