#region Libraries

using UnityEngine;

#endregion

namespace Runtime.AI.Navigation.Jobs.Pathing
{
    public readonly struct QueuedAgentRequest
    {
        #region Values

        public readonly Vector3 Destination;

        public readonly UnitAgent Agent;

        #endregion

        #region Build In States

        public QueuedAgentRequest(Vector3 destination, UnitAgent agent)
        {
            this.Destination = destination;
            this.Agent = agent;
        }

        #endregion
    }
}