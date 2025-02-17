#region Packages

using NodeCanvas.Framework;

#endregion

namespace Runtime.AI.Node_Canvas.Conditions
{
    public class CheckUnitState : ConditionTask<UnitBase>
    {
        public BBParameter<UnitState> unitState;

        protected override bool OnCheck()
            => this.agent.GetUnitState() == this.unitState.value;
    }
}