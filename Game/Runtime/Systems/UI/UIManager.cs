#region Packages

using System.Collections;
using Runtime.Battle.Systems;
using Runtime.Communication;
using Runtime.Core;
using Runtime.ScriptableVariables.Structs;
using Runtime.Systems.PersistantRunner;
using Runtime.UI_Book;
using Runtime.UI.Communication;
using Runtime.UI.Pause;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

// ReSharper disable ParameterHidesMember
namespace Runtime.Systems.UI
{
    #region Enums

    public enum UISelection
    {
        Start,
        Battle,
        Overworld,
        Pause,
        Box
    }

    #endregion

    [CreateAssetMenu(menuName = "Manager/UI")]
    public class UIManager : Manager, IFrameStart
    {
        #region Values

        private UIBook uiBook;

        [SerializeField, Required] private ChatManager chatManager;
        [SerializeField, Required] private BoolGenericVariable playerThrowingItem;

        [ShowInInspector, ReadOnly] private GameObject battleUI, overworldUI, pauseUI, startUI, loadingUI;

        [ShowInInspector, ReadOnly] private GameObject bookCanvasObject, overworldCanvasObject;

        private UISelection currentSelection = UISelection.Start;
        private bool readyToPause;

        private TextField overworldTextField;

        #endregion

        #region Build In States

        public IEnumerator FrameStart(PersistantRunner.PersistantRunner runner)
        {
            InputManager.Instance.pauseInputEvent.AddListener(this.PauseTrigger);

            while (this.bookCanvasObject == null)
            {
                this.bookCanvasObject = GameObject.Find("Book UI Canvas");
                yield return null;
            }

            while (this.overworldCanvasObject == null)
            {
                this.overworldCanvasObject = GameObject.Find("Overworld UI Canvas");
                yield return null;
            }

            this.battleUI = this.overworldCanvasObject.GetChildByName("Battle");
            this.overworldUI = this.overworldCanvasObject.GetChildByName("Overworld");
            this.pauseUI = this.bookCanvasObject.GetChildByName("Pause Menu");
            this.startUI = this.bookCanvasObject.GetChildByName("Start Menu");
            this.loadingUI = this.bookCanvasObject.GetChildByName("Loading UI");

            this.overworldTextField = this.overworldUI.GetComponentInChildren<TextField>(true);

            this.ready = true;
        }

        #endregion

        #region Getters

        public UIBook UIBook => this.uiBook;

        public GameObject CanvasObject => this.bookCanvasObject;

        #region Battle

        #endregion

        #endregion

        #region Setters

        public void SetReadyToPause(bool set) => this.readyToPause = set;

        public void SetUIBook(UIBook set) => this.uiBook = set;

        #endregion

        #region In

        public void Setup(GameObject battleUI, GameObject overworldUI, GameObject pauseUI, GameObject startUI,
            GameObject loadingUI)
        {
            this.battleUI = battleUI;
            this.overworldUI = overworldUI;
            this.pauseUI = pauseUI;
            this.startUI = startUI;
            this.loadingUI = loadingUI;
        }

        public void SwitchUI(UISelection selection)
        {
            this.battleUI.SetActive(false);
            this.overworldUI.SetActive(false);
            this.pauseUI.SetActive(false);
            this.startUI.SetActive(false);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (selection)
            {
                case UISelection.Start:
                    this.startUI.SetActive(true);
                    break;

                case UISelection.Battle:
                    this.battleUI.SetActive(true);
                    this.chatManager.SetAdaptive(false);
                    break;

                case UISelection.Overworld:
                    this.overworldUI.SetActive(true);
                    this.overworldTextField.MakeCurrent();
                    this.chatManager.ShowTextField(true);
                    this.chatManager.SetAdaptive(true);
                    break;

                case UISelection.Pause:
                    this.pauseUI.SetActive(true);
                    this.pauseUI.GetComponent<PauseMenu>().OnDisplay(this.currentSelection);
                    break;

                case UISelection.Box:
                    break;
            }

            this.currentSelection = selection;
        }

        public void ActivateLoadingUI(bool set) =>
            this.loadingUI.SetActive(set);

        public void SetBattleUI(Transform set)
        {
            for (int i = this.battleUI.transform.childCount - 1; i >= 0; i--)
                Destroy(this.battleUI.transform.GetChild(i));

            set.SetParent(this.battleUI.transform);
        }

        #endregion

        #region Internal

        private void PauseTrigger()
        {
            if (this.playerThrowingItem.Value) return;

            if (BattleSystem.instance is not null) return;

            if (!this.readyToPause) return;

            this.readyToPause = false;

            this.SwitchUI(UISelection.Pause);

            this.uiBook.Effect(BookTurn.Open);
        }

        #endregion
    }
}