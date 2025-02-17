#region Packages

using NodeCanvas.Framework;
using UnityEngine;

#endregion

namespace Runtime.AI.Node_Canvas.Actions
{
    public class AddDeltaTime : ActionTask
    {
        public BBParameter<float> counter;

        protected override void OnExecute()
        {
            this.counter.SetValue(this.counter.value + Time.deltaTime);
        }
    }
}
