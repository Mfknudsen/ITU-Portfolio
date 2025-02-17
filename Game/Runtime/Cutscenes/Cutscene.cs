#region Libraries

using Runtime.Systems;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.Core;
using UnityEngine;
using UnityEngine.Playables;

#endregion

namespace Runtime.Cutscenes
{
    [CreateAssetMenu(menuName = "Cutscenes/Basic", fileName = "Basic Cutscene")]
    public class Cutscene : ScriptableObject
    {
        #region Values

        [SerializeField] private List<PlayableAsset> timelines;
        [SerializeField] private List<DelayTime> delayTimes;

        private int currentPlayIndex;
        private bool readyForNext, waitingForInput;

        #endregion

        #region In

        public void Enable() =>
            InputManager.Instance.nextChatInputEvent.AddListener(this.OnNextChatUpdate);

        public void Disable() =>
            InputManager.Instance.nextChatInputEvent.RemoveListener(this.OnNextChatUpdate);

        public void DeployCutscene() { }

        public void TriggerNext()
        {
            if (this.delayTimes.Where(d => d.index.Equals(this.currentPlayIndex)).Select(d => d.delay).First() is float delayTime)
                new Timer(delayTime, this.PlayNext);
            else
                this.waitingForInput = true;

            this.currentPlayIndex++;
        }

        #endregion

        #region Internal

        private void PlayNext()
        {

        }

        private void OnNextChatUpdate()
        {

        }

        #endregion
    }

    [Serializable]
    internal struct DelayTime
    {
        [HorizontalGroup(" ")]
        public int index;
        [HorizontalGroup(" ")]
        public float delay;
    }
}
