#region Packages

using NodeCanvas.Framework;
using UnityEngine.AI;

#endregion

namespace Runtime.AI.Node_Canvas.Actions
{
    public class SetupSubGraph : ActionTask<NavMeshAgent>
    {
        public BBParameter<NavMeshAgent> navAgent;
        public BBParameter<UnitBase> unit;

        protected override void OnExecute()
        {
            if (this.navAgent.value != null && this.unit.value != null)
            {
                this.EndAction(true);
                return;
            }

            try
            {
                this.navAgent.value = this.agent;

                this.unit.value = this.agent.GetComponent<UnitBase>();
            }
            catch
            {
                this.EndAction(false);
                return;
            }

            this.EndAction(true);
        }
    }
}