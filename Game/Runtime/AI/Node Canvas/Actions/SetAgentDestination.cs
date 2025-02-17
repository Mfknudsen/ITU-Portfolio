#region Libraries

using NodeCanvas.Framework;
using UnityEngine;

#endregion

namespace Runtime.AI.Node_Canvas.Actions
{
    public class SetAgentDestination : ActionTask<UnitBase>
    {
        public BBParameter<Transform> target;

        protected override void OnExecute()
        {
            Transform t = this.target.value;
            this.agent.MoveAndRotateUnitAgent(t.position, t.rotation);
            this.EndAction(true);
        }
    }
}