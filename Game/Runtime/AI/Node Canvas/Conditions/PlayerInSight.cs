#region Packages

using NodeCanvas.Framework;
using Runtime.AI.Senses.Sight;

#endregion

namespace Runtime.AI.Node_Canvas.Conditions
{
    public class PlayerInSight : ConditionTask<UnitSight>
    {
        protected override bool OnCheck()
            => this.agent.IsPlayerInSight;
    }
}