#region Packages

using NodeCanvas.Framework;
using UnityEngine.AI;

#endregion

namespace Runtime.AI.Node_Canvas.Conditions
{
    public class AgentIsOnNavmesh : ConditionTask<NavMeshAgent>
    {
        protected override bool OnCheck()
        {
            return this.agent.isOnNavMesh;
        }
    }
}
