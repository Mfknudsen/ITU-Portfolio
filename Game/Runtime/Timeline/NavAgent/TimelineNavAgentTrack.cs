#region Packages

using System.ComponentModel;
using Runtime.Timeline.Mono;
using UnityEngine.AI;
using UnityEngine.Timeline;

#endregion

namespace Runtime.Timeline.NavAgent
{
    [DisplayName("Custom/Navigation Agent")]
    [TrackClipType(typeof(TimelineNavAgentClip))]
    [TrackBindingType(typeof(NavMeshAgent))]
    public class TimelineNavAgentTrack : TimelineMonoTrack
    {
        
    }
}