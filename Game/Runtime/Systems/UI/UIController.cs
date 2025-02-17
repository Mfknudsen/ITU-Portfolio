#region Packages

using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.Systems.UI
{
    public class UIController : MonoBehaviour
    {
        #region Values

        [SerializeField, Required] private UIManager uiManager;

        [SerializeField, Required, SceneObjectsOnly]
        private GameObject battleUI, overworldUI, pauseUI, startUI, loadingUI;

        #endregion

        #region Build In States

        private void Start() => this.uiManager.Setup(
            this.battleUI,
            this.overworldUI,
            this.pauseUI,
            this.startUI,
            this.loadingUI);

        #endregion
    }
}