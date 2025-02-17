#region Libraries

using System.Collections.Generic;
using Runtime.World.Overworld.Spawner;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

#endregion

namespace Editor.World.Spawner
{
    [CustomEditor(typeof(SpawnLocation))]
    public sealed class SpawnerEditor : OdinEditor
    {
        #region Values

        private static readonly float handleSize = 2, aboveFloorDistance = 1.5f;

        private static bool moveState = true;

        private readonly List<int> selectedIDs = new();

        #endregion

        #region Build In States

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Switch mode"))
            {
                moveState = !moveState;
                SceneView.RepaintAll();
            }

            SpawnLocation spawnLocation = (SpawnLocation)this.target;

            if (spawnLocation.IsAreaSpawnType)
            {
                if (GUILayout.Button("Lower Area Points to ground level"))
                    this.LowerAreaPointsToGround(spawnLocation);

                if (GUILayout.Button("Clean unused Area Points"))
                    this.CleanUpUnusedAreaPoints(spawnLocation);
            }

            if (moveState == false && spawnLocation.IsAreaSpawnType)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Create New Point"))
                {
                    spawnLocation.CreateAreaPoint();
                    Undo.RecordObject(spawnLocation, "Created new point");
                    SceneView.RepaintAll();
                }
                else if (GUILayout.Button("Remove selected point")
                    && this.selectedIDs.Count == 1
                    && spawnLocation.GetAreaPoints.Length > 3)
                {
                    if (spawnLocation.TryRemoveAreaPoint(this.selectedIDs[0]))
                    {
                        Undo.RecordObject(spawnLocation, "Removed a point");
                        SceneView.RepaintAll();
                    }
                }

                GUILayout.EndHorizontal();
            }
        }

        private void OnSceneGUI()
        {
            SpawnLocation spawnLocation = (SpawnLocation)this.target;

            if (spawnLocation.IsAreaSpawnType)
                this.AreaTools(Event.current, spawnLocation);
            else
                this.LocationTools(spawnLocation);
        }

        #endregion

        #region Internal

        private void LocationTools(SpawnLocation spawnLocation)
        {
            foreach (Transform t in spawnLocation.GetLocationPoints)
            {
                var fmh_94_65_638231264529049304 = Quaternion.identity; t.position = Handles.FreeMoveHandle(t.position, handleSize, Vector3.zero, Handles.SphereHandleCap);

                var fmh_96_87_638231264529085418 = Quaternion.identity; Vector3 target = Handles.FreeMoveHandle(t.position + t.forward * .5f, handleSize, Vector3.zero, Handles.SphereHandleCap);
                target = new(target.x, t.position.y, target.z);

                t.LookAt(target);
            }
        }

        private void AreaTools(Event guiEvent, SpawnLocation spawnLocation)
        {
            Vector3[] positions = spawnLocation.GetAreaPoints;

            if (moveState)
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    var fmh_111_80_638231264529094318 = Quaternion.identity; Vector3 newPosition = Handles.FreeMoveHandle(positions[i], handleSize, Vector3.zero, Handles.SphereHandleCap);

                    if (newPosition == positions[i]) continue;

                    Undo.RecordObject(spawnLocation, "Moved point");
                    spawnLocation.SetAreaPointPosition(i, newPosition);
                }
            }
            else
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    Handles.color = this.selectedIDs.Contains(i) ? Color.green : Color.red;

                    bool clicked = Handles.Button(positions[i], Quaternion.identity, handleSize, handleSize, Handles.SphereHandleCap);

                    if (!clicked) continue;

                    if (guiEvent.shift && !this.selectedIDs.Contains(i))
                        this.selectedIDs.Add(i);
                    else if (!guiEvent.shift && !guiEvent.control)
                    {
                        this.selectedIDs.Clear();
                        this.selectedIDs.Add(i);
                    }
                    else if (guiEvent.control && !guiEvent.shift)
                        this.selectedIDs.Remove(i);
                }

                if (guiEvent.shift && this.selectedIDs.Count == 3 && !guiEvent.control)
                {
                    if (guiEvent.keyCode == KeyCode.E)
                    {
                        if (spawnLocation.TryCreateNewAreaTriangle(this.selectedIDs.ToArray()))
                            Undo.RecordObject(spawnLocation, "Created new triangle");
                    }
                    else if (guiEvent.keyCode == KeyCode.W)
                    {
                        if (spawnLocation.TryRemoveAreaTriangle(this.selectedIDs.ToArray()))
                        {
                            this.selectedIDs.Clear();
                            Undo.RecordObject(spawnLocation, "Removed a triangle");
                        }
                    }
                }
            }
        }

        private void LowerAreaPointsToGround(SpawnLocation spawnLocation)
        {
            LayerMask layerMask = LayerMask.GetMask("Environment");
            Vector3[] points = spawnLocation.GetAreaPoints;
            for (int i = 0; i < points.Length; i++)
            {
                if (Physics.Raycast(points[i], -Vector3.up, out RaycastHit hit, Mathf.Infinity, layerMask, QueryTriggerInteraction.Ignore))
                    spawnLocation.SetAreaPointPosition(i, hit.point + Vector3.up * aboveFloorDistance);
            }

            Undo.RecordObject(spawnLocation, "Lowered Area Points to ground level");
        }

        private void CleanUpUnusedAreaPoints(SpawnLocation spawnLocation)
        {
            if (spawnLocation.TryCleanAreaPoints())
            {
                SceneView.RepaintAll();
                Undo.RecordObject(spawnLocation, "Cleaned unused Area Points");
            }
        }

        #endregion
    }
}