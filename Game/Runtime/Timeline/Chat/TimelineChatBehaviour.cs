#region Packages

using Runtime.Communication;
using UnityEngine.Playables;

#endregion

namespace Runtime.Timeline.Chat
{
    public class TimelineChatBehaviour : PlayableBehaviour
    {
        #region Values

        public ChatManager chatManager;
        public Communication.Chat clip;

        #endregion

        #region Build In States

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            this.chatManager.Add(this.clip);
        }

        #endregion
    }
}