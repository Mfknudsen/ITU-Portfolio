#region Packages

using NodeCanvas.Framework;

#endregion

namespace Runtime.AI.Node_Canvas.Actions
{
    public class SetStructFromState<T> : ActionTask<UnitBase> where T : struct
    {
        public BBParameter<string> key;
        public BBParameter<T> toSet;

        protected override void OnExecute()
        {
            object o = this.agent.GetStateByKey(this.key.GetValue());

            if (o is T t)
            {
                this.toSet.SetValue(t);

                this.EndAction(true);
            }

            this.EndAction(false);
        }
    }
}