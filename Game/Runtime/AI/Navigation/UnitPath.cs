#region Libraries

using System;
using System.Collections.Generic;
using Runtime.AI.Navigation.PathActions;
using Runtime.Algorithms.PathFinding;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.AI.Navigation
{
    [Serializable]
    public struct UnitPath
    {
        #region Values

        [ShowInInspector] private int actionIndex;
        [ShowInInspector] private readonly List<PathAction> actions;
        [ShowInInspector] private readonly List<int> triangleIDs;

        [ShowInInspector] private readonly Vector3 endPoint;

        #endregion

        #region Build In States

        public UnitPath(Vector3 startPoint, Vector3 endPoint, int[] pathTriangleIDs, NavTriangle[] triangles,
            Vector3[] verts,
            Vector2[] simpleVerts, NavigationAgent agent)
        {
            this.actionIndex = 0;
            this.endPoint = endPoint;
            this.actions = new List<PathAction>();
            this.triangleIDs = new List<int>();

            foreach (int pathTriangleID in pathTriangleIDs)
                this.triangleIDs.Add(pathTriangleID);

            foreach (Vector3 point in Funnel.GetPath(startPoint, endPoint, pathTriangleIDs, triangles, verts, agent))
                this.actions.Add(new WalkAction(point));
        }

        #endregion

        #region Getters

        public readonly Vector3 Destination()
        {
            return this.endPoint;
        }

        public readonly int ActionIndex()
        {
            return this.actionIndex;
        }

        public PathAction GetCurrentPathAction()
        {
            return this.actions[this.actionIndex];
        }

        public readonly int MaxIndex()
        {
            return this.actions.Count - 1;
        }

        public readonly Vector3 GetActionDestinationByIndex(int index)
        {
            return this.actions[index].Destination();
        }

#if UNITY_EDITOR
        public List<int> GetTriangleIDs()
        {
            return this.triangleIDs;
        }
#endif

        #endregion

        #region Out

        public readonly bool Empty => this.actions == null || this.actions.Count == 0;

        public readonly bool Complete => this.actions != null && this.actions.Count == this.actionIndex;

        #endregion

        #region In

#if UNITY_EDITOR
        public void DebugPath(NavigationAgent agent)
        {
            if (this.Empty || !UnitNavigation.Ready)
                return;

            Debug.DrawLine(agent.transform.position, this.actions[0].Destination(), Color.yellow);
            for (int i = 1; i < this.actions.Count; i++)
                Debug.DrawLine(this.actions[i].Destination(), this.actions[i - 1].Destination(), Color.yellow);
        }
#endif

        public void CheckIndex(NavigationAgent agent)
        {
            if (this.actions[this.actionIndex].CheckAction(this, agent))
                this.actionIndex++;
        }

        #endregion
    }
}