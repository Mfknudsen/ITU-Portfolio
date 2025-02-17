#region Libraries

using System.Collections.Generic;

#endregion

namespace Runtime.AI.Navigation
{
    public readonly struct NavMeshCell
    {
        #region Values

        private readonly List<int> agentIDs;
        private readonly List<int> triangleIDs;

        #endregion

        #region Build In States

        public NavMeshCell(List<int> triangleIDs)
        {
            this.agentIDs = new List<int>();
            this.triangleIDs = triangleIDs;
        }

        #endregion

        #region Getters

        public List<int> GetTriangleIDs()
        {
            return this.triangleIDs;
        }

        public List<int> GetAgentIDs()
        {
            return this.agentIDs;
        }

        #endregion

        #region In

        public void AddAgentID(int id)
        {
            if (!this.agentIDs.Contains(id))
                this.agentIDs.Add(id);
        }

        public void RemoveAgentID(int id)
        {
            this.agentIDs.Remove(id);
        }

        #endregion
    }
}