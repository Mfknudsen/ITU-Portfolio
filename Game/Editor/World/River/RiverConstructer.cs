#region Libraries

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Editor.World.River
{
    public sealed class RiverConstructor : MonoBehaviour
    {
        #region Values

        [SerializeField, HideInInspector] private List<Vector3> riverPoints = new List<Vector3>();

        [SerializeField, HideInInspector] private List<float> widths = new List<float>();

        [SerializeField, HideInInspector] private List<Angles> pointAngles = new List<Angles>();

        #endregion

        #region In

        public void AddPointForwards()
        {
        }

        public void AddPointBackwards()
        {
        }

        public bool DeleteAt(int i)
        {
            if (i < 0 || i >= this.riverPoints.Count)
                return false;

            this.riverPoints.RemoveAt(i);
            this.widths.RemoveAt(i);
            this.pointAngles.RemoveAt(i);

            return true;
        }

        #endregion
    }

    [Serializable]
    internal struct Angles
    {
        #region Values

        [SerializeField]
        private Vector3 forward, backwards;

        #endregion

        #region Build In States

        public Angles(Vector3 forward, Vector3 backwards)
        {
            this.forward = forward;
            this.backwards = backwards;
        }

        #endregion
    }
}