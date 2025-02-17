#region Packages

using System.Collections;
using Runtime.World;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.Menu.StartMenu
{
    public class MainMenu : MonoBehaviour
    {
        #region Values

        [SerializeField, Required] private WorldManager worldManager;
        
        private bool ready = true;

        #endregion

        #region In

        public void LoadScene(string sceneName)
        {
            if(!this.ready) return;

            this.ready = true;

            this.StartCoroutine(this.Load(sceneName));
        }

        public void StartNewGame()
        {
            if (!this.ready) return;

            this.ready = true;

            this.StartCoroutine(this.Load("Shayklind"));
        }

        #endregion

        #region Internal

        private IEnumerator Load(string sceneName)
        {
            if (!this.worldManager.GetCurrentLoadedWorldScene().Equals(sceneName))
                this.worldManager.LoadSceneAsync(sceneName);

            yield return null;

            yield return new WaitWhile(() => this.worldManager.GetIsLoading());

            this.worldManager.UnloadSceneAsync("StartMenu");

            yield return null;

            yield return new WaitWhile(() => this.worldManager.GetActiveUnloading());
        }

        #endregion
    }
}