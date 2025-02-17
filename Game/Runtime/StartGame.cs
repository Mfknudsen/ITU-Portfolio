#region Libraries

using Runtime.Player;
using Runtime.Systems.UI;
using Runtime.UI_Book;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#endregion

namespace Runtime
{
    public sealed class StartGame : MonoBehaviour
    {
        #region Values

        [SerializeField, Required] private PlayerManager playerManager;
        [SerializeField, Required] private UIManager uiManager;

        #endregion

        private IEnumerator Start()
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("UI", LoadSceneMode.Additive);

            yield return new WaitWhile(() => !asyncOperation.isDone);

            yield return new WaitUntil(() => this.playerManager.Ready && this.uiManager.Ready);

            this.playerManager.DisablePlayerControl();

            this.uiManager.SwitchUI(UISelection.Start);
            UIBook.Instance.ConstructUI();
        }
    }
}