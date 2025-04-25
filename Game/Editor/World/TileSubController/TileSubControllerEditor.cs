#region Libraries

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif
using UnityEditor;
using UnityEngine;

#endregion

namespace Editor.World.TileSubController
{
#if UNITY_EDITOR
    [CustomEditor(typeof(Runtime.World.Overworld.TileSubController))]
    public sealed class TileSubControllerEditor : OdinEditor
    {
        #region Values

        private Runtime.World.Overworld.TileSubController controller;

        #endregion

        #region Build In States

        protected override void OnEnable()
        {
            base.OnEnable();

            this.controller = this.target as Runtime.World.Overworld.TileSubController;
        }

        private void OnSceneGUI()
        {
            Handles.Label(this.controller.GetCleanUpPoint - Vector3.up * .15f, "NavMesh Clean Point");
            this.controller.SetCleanUpPoint(
                Handles.PositionHandle(this.controller.GetCleanUpPoint, Quaternion.identity));
        }

        #endregion
    }
#endif
}