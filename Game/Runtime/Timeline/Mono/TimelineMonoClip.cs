#region Packages

using UnityEngine;
using UnityEngine.Playables;

#endregion

namespace Runtime.Timeline.Mono
{
    public class TimelineMonoClip<TBehaviour> : PlayableAsset where TBehaviour : TimelineMonoBehaviour, new()
    {
        #region Values

        public bool enable;
        public string functionToTrigger;
        
        #endregion
        
        #region Build In States

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            ScriptPlayable<TBehaviour> playable =
                ScriptPlayable<TBehaviour>.Create(graph);

            this.TransferValues(playable.GetBehaviour());
            
            return playable;
        }

        #endregion

        #region Internal

        protected virtual void TransferValues(TBehaviour behaviour)
        {
            behaviour.enable = this.enable;
            behaviour.functionToTrigger = this.functionToTrigger;
        }

        #endregion
    }
}
