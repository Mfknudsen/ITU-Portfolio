#region Packages

using NodeCanvas.Framework;
using UnityEngine;

#endregion

namespace Runtime.AI.Node_Canvas.Conditions
{
    public class TargetIsPlayer : ConditionTask
    {
        public BBParameter<GameObject> target;

        protected override bool OnCheck()
        {
            return this.target.value.transform.root.name.Equals("Player");
        }
    }
}