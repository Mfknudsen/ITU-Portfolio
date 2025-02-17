#region Packages

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Runtime.AI.World
{
    public class Waypoints : MonoBehaviour
    {
        #region Values

        private List<Transform> points = new List<Transform>();

        #endregion

        #region Build In States

        private void OnDrawGizmos()
        {
            if (this.transform.childCount < 2)
                return;

            for (int i = 0; i < this.transform.childCount - 1; i++)
                Debug.DrawLine(this.transform.GetChild(i).position, this.transform.GetChild(i + 1).position, Color.red);
        }

        private void Start()
        {
            foreach (Transform t in this.transform) this.points.Add(t);
        }

        #endregion

        #region Getters

        public Transform[] GetPoints()
        {
            return this.points.ToArray();
        }

        #endregion
    }
}