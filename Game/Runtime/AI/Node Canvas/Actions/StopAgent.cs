#region Packages

using NodeCanvas.Framework;
using UnityEngine.AI;

#endregion

namespace Runtime.AI.Node_Canvas.Actions
{
    public class StopAgent : ActionTask<NavMeshAgent>
    {
        protected override void OnExecute()
        {
            this.agent.isStopped = true;

            this.EndAction(true);
        }
    }
}