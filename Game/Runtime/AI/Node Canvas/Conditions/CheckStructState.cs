#region Packages

using NodeCanvas.Framework;

#endregion

namespace Runtime.AI.Node_Canvas.Conditions
{
    public class CheckStructState<T> : ConditionTask where T : struct
    {
        public BBParameter<string> key;
        public BBParameter<T> checkAgainst;

        protected override bool OnCheck()
        {
            return this.checkAgainst.value.Equals(this.agent.GetComponent<UnitBase>().GetStateByKey(this.key.GetValue()));
        }
    }
}
