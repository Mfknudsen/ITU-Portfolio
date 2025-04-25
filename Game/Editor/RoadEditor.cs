#region Packages

using Runtime.Core.CorePath.Road;
using UnityEditor;
using UnityEngine;

#endregion

namespace Editor
{
#if UNITY_EDITOR
    [CustomEditor(typeof(RoadCreator))]
    public class RoadEditor : UnityEditor.Editor
    {
        private RoadCreator creator;

        private void OnSceneGUI()
        {
            if (this.creator.autoUpdate && Event.current.type == EventType.Repaint) this.creator.UpdateRoad();
        }

        private void OnEnable()
        {
            this.creator = (RoadCreator)this.target;
        }
    }
#endif
}