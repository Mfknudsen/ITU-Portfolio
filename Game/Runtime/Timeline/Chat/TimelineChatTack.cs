#region Packages

using System.ComponentModel;
using UnityEngine.Timeline;

#endregion

namespace Runtime.Timeline.Chat
{
    [DisplayName("Custom/Chat")]
    [TrackClipType(typeof(TimelineChatClip))]
    public class TimelineChatTack : TrackAsset
    {
    }
}
