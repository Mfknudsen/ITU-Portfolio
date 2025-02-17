#region Libraries

using System.Collections.Generic;
using Runtime.AI.Navigation;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.Editor.Tests
{
    public sealed class SavedUnitPath : SerializedScriptableObject
    {
        #region Values

        [SerializeField] private List<Vector3> pathPoints;

        [SerializeField] private Vector3 start, end;

        #endregion

        #region Getters

        public List<Vector3> GetPathPoints() => this.pathPoints;

        public Vector3 GetStart() => this.start;

        public Vector3 GetEnd() => this.end;

        #endregion

        #region In

        public void SetValues(UnitPath path, Vector3 s, Vector3 e)
        {
            this.start = s;
            this.end = e;

            this.pathPoints = new List<Vector3>();
            foreach (int triangleID in path.GetTriangleIDs())
            {
                foreach (int vertex in UnitNavigation.GetTriangleByID(triangleID).Vertices)
                    this.pathPoints.Add(UnitNavigation.Get3DVertByIndex(vertex));
            }
        }

        #endregion
    }
}