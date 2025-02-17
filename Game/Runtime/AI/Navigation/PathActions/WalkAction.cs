#region Libraries

using Runtime.Core;
using UnityEngine;

#endregion

namespace Runtime.AI.Navigation.PathActions
{
    public sealed class WalkAction : PathAction
    {
        #region Values

        private readonly Vector3 destination;

        #endregion

        #region Build In States

        public WalkAction(Vector3 destination)
        {
            this.destination = destination;
        }

        #endregion

        #region Getters

        public override Vector3 Destination()
        {
            return this.destination;
        }

        public override bool IsWalkAction()
        {
            return true;
        }

        #endregion

        #region In

        public override bool CheckAction(UnitPath path, UnitAgent agent)
        {
            Vector2 agentPosition = agent.transform.position.XZ(),
                destinationXZ = this.destination.XZ();

            if (path.ActionIndex() >= path.MaxIndex())
                return agentPosition.QuickSquareDistance(destinationXZ) <
                       agent.Settings.StoppingDistance * agent.Settings.StoppingDistance;

            Vector2 next = path.GetActionDestinationByIndex(path.ActionIndex() + 1).XZ();

            if (path.ActionIndex() == 0)
                return agentPosition.QuickSquareDistance(next) <
                       destinationXZ.QuickSquareDistance(next);

            Vector2 previous = path.GetActionDestinationByIndex(path.ActionIndex() - 1).XZ();

            return agentPosition.QuickSquareDistance(next) <
                   destinationXZ.QuickSquareDistance(next) &&
                   agentPosition.QuickSquareDistance(previous) >
                   destinationXZ.QuickSquareDistance(previous);
        }

        #endregion
    }
}