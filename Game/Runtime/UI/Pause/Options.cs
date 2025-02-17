#region Packages

using JetBrains.Annotations;
using UnityEngine;

#endregion

namespace Runtime.UI.Pause
{
    public class Options : MonoBehaviour
    {
        #region Values

        [SerializeField] private GameObject gameplayTab, audioTab, screenTab;

        #endregion

        #region In

        [UsedImplicitly]
        public void Gameplay()
        {
            this.gameplayTab.SetActive(true);

            this.audioTab.SetActive(false);
            this.screenTab.SetActive(false);
        }

        [UsedImplicitly]
        public void Audio()
        {
            this.audioTab.SetActive(true);

            this.gameplayTab.SetActive(false);
            this.screenTab.SetActive(false);
        }

        [UsedImplicitly]
        public void Screen()
        {
            this.screenTab.SetActive(true);

            this.gameplayTab.SetActive(false);
            this.audioTab.SetActive(false);
        }

        #endregion
    }
}