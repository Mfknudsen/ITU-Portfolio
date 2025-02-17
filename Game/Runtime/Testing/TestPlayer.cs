#region Libraries

using Runtime.Player;
using Runtime.Systems;
using Runtime.Systems.PersistantRunner;
using Runtime.Systems.UI;
using System;
using System.Collections;
using System.Linq;
using Runtime.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

#endregion

namespace Runtime.Testing
{
    public sealed class TestPlayer : MonoBehaviour
    {
#if UNITY_EDITOR
        #region Values

        [SerializeField] private bool use;
        [SerializeField] private Manager[] managers;
        [SerializeField] private GameObject playerPrefab;

        #endregion

        private IEnumerator Start()
        {
            if (!this.use)
                yield break;

            PlayerManager playerManager = this.managers.First(m => m is PlayerManager) as PlayerManager;
            if (playerManager.GetAgent() != null)
                Destroy(this.gameObject);

            UIManager uiManager = this.managers.First(m => m is UIManager) as UIManager;

            if (playerManager == null)
                throw new Exception("Missing Player Manager");
            if (uiManager == null)
                throw new Exception("Missing UI Manager");

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("UI", LoadSceneMode.Additive);

            Transform t = this.transform;
            Vector3 position = t.position;
            Quaternion rotation = t.rotation;
            Instantiate(this.playerPrefab, position, rotation);
            GameObject persistent = new GameObject("Persistent Runner");
            //persistent.AddComponent<TimerUpdater>();
            PersistantRunner runner = persistent.AddComponent<PersistantRunner>();
            runner.AddManagers(this.managers);

            yield return new WaitWhile(() =>
                !asyncOperation.isDone);

            yield return new WaitUntil(() =>
                playerManager.Ready && uiManager.Ready);

            uiManager.SwitchUI(UISelection.Overworld);
            uiManager.SetReadyToPause(false);
            playerManager.EnablePlayerControl();
            playerManager.SetState(PlayerState.Default);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
#endif
    }
}