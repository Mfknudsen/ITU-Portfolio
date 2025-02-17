#region Packages

using Runtime.Cutscenes;
using Runtime.Systems;
using UnityEngine;
using UnityEngine.Playables;

#endregion

namespace Runtime.Timeline
{
    [CreateAssetMenu(menuName = "Manager/Cutscene")]
    public class CutsceneManager : Manager
    {
        #region Values

        private PlayableDirector director;

        private Cutscene currentPlaying;

        #endregion

        #region In

        public void PlayCutscene(Cutscene toPlay)
        {
            if (toPlay == null)
                return;

            if (this.currentPlaying != null) this.currentPlaying.Disable();

            this.currentPlaying = toPlay;
            this.currentPlaying.Enable();
        }

        #endregion
    }
}