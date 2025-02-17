#region Packages

using Runtime.Timeline.Mono;
using UnityEngine;

#endregion

namespace Runtime.Timeline.NavAgent
{
    public class TimelineNavAgentClip : TimelineMonoClip<TimelineNavAgentBehaviour>
    {
        #region Values

        public Transform toMoveTo;
        public bool isStopped;
        public float stopDistance;

        #endregion

        #region Internal

        protected override void TransferValues(TimelineNavAgentBehaviour behaviour)
        {
            base.TransferValues(behaviour);

            behaviour.toMoveTo = this.toMoveTo;
            behaviour.isStopped = this.isStopped;
            behaviour.stopDistance = this.stopDistance;
        }

        #endregion
    }
}