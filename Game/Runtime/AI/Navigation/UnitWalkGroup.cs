#region Libraries

using System.Collections.Generic;
using Runtime.Core;
using UnityEngine;

#endregion

namespace Runtime.AI.Navigation
{
    public struct UnitWalkGroup
    {
        #region Values

        private readonly List<UnitAgent> agents;

        private Vector2 center, direction;

        private int id;

        #endregion

        #region Build In States

        public UnitWalkGroup(UnitAgent agent1, UnitAgent agent2, int id)
        {
            this.agents = new List<UnitAgent>();
            this.center = (agent1.transform.position.XZ() + agent2.transform.position.XZ()) * .5f;
            this.direction = (agent1.GetCurrentNavMeshDirection() + agent2.GetCurrentNavMeshDirection()) * .5f;
            this.id = id;
            this.AddAgent(agent1);
            this.AddAgent(agent2);
        }

        #endregion

        #region Getters

        public Vector2 GetCenter()
        {
            return this.center;
        }

        public Vector2 GetDirection()
        {
            return this.direction;
        }

        public List<UnitAgent> GetAgents()
        {
            return this.agents;
        }

        public int GetID()
        {
            return this.id;
        }

        #endregion

        #region Setters

        public void SetID(int set)
        {
            this.id = set;
            foreach (UnitAgent unitAgent in this.agents)
                unitAgent.SetGroupID(this.id);
        }

        #endregion

        #region In

        public void Update()
        {
            this.center = Vector2.zero;
            this.direction = Vector2.zero;

            if (this.agents.Count == 0)
            {
                Debug.Log(0);
                return;
            }

            foreach (UnitAgent agent in this.agents)
            {
                this.center += agent.transform.position.XZ();
                this.direction += agent.GetCurrentNavMeshDirection();
            }

            this.center /= this.agents.Count;
            this.direction /= this.agents.Count;
        }

        public void AddAgent(UnitAgent agent)
        {
            if (!this.agents.Contains(agent))
                this.agents.Add(agent);
            agent.SetGroupID(this.id);
        }

        public bool RemoveAgent(UnitAgent agent)
        {
            this.agents.Remove(agent);
            agent.SetGroupID(-1);
            if (this.agents.Count > 1) return false;

            while (this.agents.Count > 0)
            {
                this.agents[0].SetGroupID(-1);
                this.agents.RemoveAt(0);
            }

            return true;
        }

        #endregion
    }
}