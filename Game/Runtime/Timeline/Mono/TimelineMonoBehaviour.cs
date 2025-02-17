#region Packages

using UnityEngine;
using UnityEngine.Playables;

#endregion

namespace Runtime.Timeline.Mono
{
    public class TimelineMonoBehaviour : PlayableBehaviour
    {
        #region Values

        public bool enable;
        public string functionToTrigger;

        #endregion

        #region Build In States

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            MonoBehaviour monoBehaviour = playerData as MonoBehaviour;
           
            if(monoBehaviour == null) return;
            
            monoBehaviour.enabled = this.enable;
            
            if(!this.functionToTrigger.Equals(""))
                monoBehaviour.SendMessage(this.functionToTrigger);
        }

        #endregion
    }
}
