#region Packages

using UnityEngine;

#endregion

namespace Runtime.Core.CorePath
{
    public class PathCreator : MonoBehaviour {

        [HideInInspector]
        public Path path;

        public Color anchorCol = Color.red;
        public Color controlCol = Color.white;
        public Color segmentCol = Color.green;
        public Color selectedSegmentCol = Color.yellow;
        public float anchorDiameter = .1f;
        public float controlDiameter = .075f;
        public bool displayControlPoints = true;

        public void CreatePath()
        {
            this.path = new Path(this.transform.position);
        }

        private void Reset()
        {
            this.CreatePath();
        }
    }
}