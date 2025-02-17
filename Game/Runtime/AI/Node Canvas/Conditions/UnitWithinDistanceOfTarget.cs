#region Packages

using NodeCanvas.Framework;
using UnityEngine;

#endregion

namespace Runtime.AI.Node_Canvas.Conditions
{
    public class UnitWithinDistanceOfTarget : ConditionTask<Transform>
    {
        public BBParameter<Transform> target;
        public BBParameter<float> distance;

        protected override bool OnCheck()
            => (this.agent.position - this.target.value.position).sqrMagnitude < (this.distance.value * this.distance.value);
    }
}
