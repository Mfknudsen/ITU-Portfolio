#region Packages

using System.Collections;
using System.Collections.Generic;
using Runtime.Files;
using Runtime.Player;
using Runtime.Systems;
using Runtime.Systems.PersistantRunner;
using Runtime.Systems.UI;
using Runtime.UI.SceneTransitions;
using Runtime.UI.SceneTransitions.Transitions;
using Runtime.World.Overworld;
using Runtime.World.Overworld.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Logger = Runtime.Testing.Logger;

#endregion

namespace Runtime.World
{
    [CreateAssetMenu(menuName = "Manager/World")]
    public class WorldManager : Manager, IFrameStart
    {
        #region Values

        [SerializeField, Required] private PlayerManager playerManager;
        [SerializeField, Required] private UIManager uiManager;
        [SerializeField, Required] private TileManager tileManager;

        private Coroutine currentOperation;

        [SerializeField] private float progressMeter;

        [SerializeField] private string currentLoadedBattleScene, currentLoadedWorldScene;

        private Transition transition;

        private readonly List<Coroutine> activeLoading = new List<Coroutine>(), activeUnloading = new List<Coroutine>();

        private StoryTriggers storyTriggers;
        private const string FileName = "StoryTriggers";

        private readonly List<GameObject> loadedScenes = new List<GameObject>();

        private PersistantRunner persistantRunner;
        
        #endregion

        #region Build In States

        public IEnumerator FrameStart(PersistantRunner runner)
        {
            this.storyTriggers = FileManager.LoadData<StoryTriggers>(FileName);

            this.persistantRunner = runner;
            
            this.ready = true;
            yield break;
        }

        #endregion

        #region Getters

        public float GetLoadMeter() =>
            this.progressMeter;

        public bool GetEmpty() =>
            this.currentOperation == null;

        public bool GetIsLoading() =>
            this.activeLoading.Count == 0;

        public bool GetActiveUnloading() =>
            this.activeUnloading.Count == 0;

        public string GetCurrentLoadedWorldScene() =>
            this.currentLoadedWorldScene;

        #endregion

        #region Setters

        public void SetTransition(Transition set)
        {
            this.transition = set;

            this.transition.SetTransitionParent(SceneTransitionUI.instance);
        }

        #endregion

        #region In

        public void LoadSceneAsync(string sceneName)
        {
            this.activeLoading.Add(
                this.persistantRunner.StartCoroutine(this.LoadWorldSceneAsync(sceneName)));
        }

        public void UnloadSceneAsync(string sceneName)
        {
            this.activeUnloading.Add(
                this.persistantRunner.StartCoroutine(this.UnloadWorldSceneAsync(sceneName)));
        }

        public void LoadBattleScene(string sceneName)
        {
            this.activeLoading.Add(
                this.persistantRunner.StartCoroutine(this.LoadBattleSceneAsync(sceneName)));
        }

        public void UnloadCurrentBattleScene()
        {
            this.activeUnloading.Add(
                this.persistantRunner.StartCoroutine(this.UnloadBattleSceneAsync(this.currentLoadedBattleScene)));
        }

        #endregion

        #region Internal

        #region Load/Unload Battle Scenes

        private IEnumerator LoadBattleSceneAsync(string sceneName)
        {
            Logger.AddLog(this.ToString(), "Loading Battle Scene Async: \n" + sceneName);

            //Start Transition
            yield return this.StartTransition();

            this.playerManager.DisableOverworld();

            //Scene Loading
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            this.progressMeter = 0;

            if (asyncLoad == null)
            {
                Debug.LogWarning("No scene loaded");
                yield break;
            }

            while (!asyncLoad.isDone)
            {
                this.progressMeter = asyncLoad.progress + 0.1f;
                yield return null;
            }

            this.currentLoadedBattleScene = sceneName;

            this.playerManager.EnableOverworld();

            //End Transition
            yield return this.EndTransition();
        }

        private IEnumerator UnloadBattleSceneAsync(string sceneName)
        {
            Logger.AddLog(this.ToString(), "Unloading Battle Scene Async: \n" + sceneName);

            //Start Transition
            yield return this.StartTransition();

            //Scene Unloading
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);

            this.progressMeter = 0;

            while (!asyncUnload.isDone)
            {
                this.progressMeter = (int)(asyncUnload.progress + 0.1f) * 100;
                yield return null;
            }

            this.currentOperation = null;

            //End Transition
            yield return this.EndTransition();

            this.playerManager.EnableOverworld();
        }

        #endregion

        #region Load/Unload World Scenes

        private IEnumerator LoadWorldSceneAsync(string sceneName)
        {
            Logger.AddLog(this.ToString(), "Loading World Scene Async: \n" + sceneName);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            this.progressMeter = 0;

            this.uiManager.ActivateLoadingUI(true);

            while (!asyncLoad.isDone)
            {
                this.progressMeter = asyncLoad.progress + 0.1f;
                yield return null;
            }

            this.uiManager.ActivateLoadingUI(false);

            this.currentOperation = null;
        }

        private IEnumerator UnloadWorldSceneAsync(string sceneName)
        {
            Logger.AddLog(this.ToString(), "Unloading World Scene Async: \n" + sceneName);

            this.UnloadWorldInterfaces(sceneName);

            AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(sceneName);

            while (!asyncOperation.isDone)
                yield return null;

            this.currentOperation = null;
        }

        #endregion

        #region Transitions

        private IEnumerator StartTransition()
        {
            yield return this.transition.Trigger(true);
        }

        private IEnumerator EndTransition()
        {
            yield return this.transition.Trigger(false);
        }

        #endregion

        private void UnloadWorldInterfaces(string sceneName)
        {
            List<GameObject> toUnload = new List<GameObject>();

            TileSubController subController = this.tileManager.GetSubManagerByName(sceneName);
            if (subController is not null)
                toUnload.Add(subController.gameObject);

            while (toUnload.Count > 0)
            {
                GameObject obj = toUnload[0];

                foreach (IUnload i in obj.GetComponents<IUnload>())
                    i.Unload();

                for (int i = 0; i < obj.transform.childCount; i++)
                    toUnload.Add(obj.transform.GetChild(i).gameObject);

                toUnload.RemoveAt(0);
            }
        }

        #endregion
    }
}