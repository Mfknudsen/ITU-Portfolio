#region Libraries

using Unity.Mathematics;

#endregion

namespace Runtime.AI.Navigation.Jobs.Pathing
{
    public readonly struct JobAgent
    {
        #region Values

        public readonly int currentTriangleID, destinationTriangleID;

        public readonly float3 endPoint, startPosition;

        public readonly float radius;

        #endregion

        #region Build In States

        public JobAgent(QueuedAgentRequest request)
        {
            UnitAgent agent = request.Agent;
            UnitAgentSettings settings = agent.Settings;

            this.currentTriangleID = agent.CurrentTriangleIndex();
            this.endPoint = request.Destination;
            this.startPosition = agent.transform.position;
            this.destinationTriangleID = UnitNavigation.ClosestTriangleIndex(this.endPoint);
            this.radius = settings.Radius;
        }

        #endregion
    }
}