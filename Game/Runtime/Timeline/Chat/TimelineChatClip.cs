#region Packages

using UnityEngine;
using UnityEngine.Playables;

#endregion

namespace Runtime.Timeline.Chat
{
    public class TimelineChatClip : PlayableAsset
    {
        #region Values

        public Communication.Chat clip;

        #endregion

        #region Build In States

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            ScriptPlayable<TimelineChatBehaviour> playable = ScriptPlayable<TimelineChatBehaviour>.Create(graph);

            TimelineChatBehaviour timelineChatBehaviour = playable.GetBehaviour();
            timelineChatBehaviour.clip = this.clip;
            
            return playable;
        }

        #endregion
    }
}
