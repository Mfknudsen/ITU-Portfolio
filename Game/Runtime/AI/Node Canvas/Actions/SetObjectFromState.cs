#region Packages

using NodeCanvas.Framework;
using UnityEngine;

#endregion

namespace Runtime.AI.Node_Canvas.Actions
{
    public class SetObjectFromState<T> : ActionTask<UnitBase> where T : Object
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