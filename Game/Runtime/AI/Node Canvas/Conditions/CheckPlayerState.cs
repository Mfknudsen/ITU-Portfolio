#region Packages

using NodeCanvas.Framework;
using Runtime.Player;

#endregion

namespace Runtime.AI.Node_Canvas.Conditions
{
    public class CheckPlayerState : ConditionTask
    {
        public BBParameter<PlayerManager> playManager;
        public BBParameter<PlayerState> targetState;

        protected override bool OnCheck()
            => this.playManager.value.GetPlayerState() == this.targetState.value;
    }
}