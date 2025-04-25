#region Libraries

using System.Collections.Generic;
using Runtime.World.Overworld.Spawner;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif
using UnityEditor;
using UnityEngine;

#endregion

namespace Editor.World.Spawner
{
#if UNITY_EDITOR
    [CustomEditor(typeof(SpawnLocation))]
    public sealed class SpawnerEditor : OdinEditor
    {
        #region Values

        private const float HandleSize = 2, AboveFloorDistance = 1.5f;

        private static bool moveState = true;

        private readonly List<int> selectedIDs = new List<int>();

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
                    LowerAreaPointsToGround(spawnLocation);

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
                LocationTools(spawnLocation);
        }

        #endregion

        #region Internal

        private static void LocationTools(SpawnLocation spawnLocation)
        {
            foreach (Transform t in spawnLocation.GetLocationPoints)
            {
                t.position = Handles.FreeMoveHandle(t.position, HandleSize, Vector3.zero, Handles.SphereHandleCap);

                Vector3 targetPosition = Handles.FreeMoveHandle(t.position + t.forward * .5f, HandleSize, Vector3.zero,
                    Handles.SphereHandleCap);
                targetPosition = new Vector3(targetPosition.x, t.position.y, targetPosition.z);

                t.LookAt(targetPosition);
            }
        }

        private void AreaTools(Event guiEvent, SpawnLocation spawnLocation)
        {
            Vector3[] positions = spawnLocation.GetAreaPoints;

            if (moveState)
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    Vector3 newPosition =
                        Handles.FreeMoveHandle(positions[i], HandleSize, Vector3.zero, Handles.SphereHandleCap);

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

                    bool clicked = Handles.Button(positions[i], Quaternion.identity, HandleSize, HandleSize,
                        Handles.SphereHandleCap);

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

        private static void LowerAreaPointsToGround(SpawnLocation spawnLocation)
        {
            LayerMask layerMask = LayerMask.GetMask("Environment");
            Vector3[] points = spawnLocation.GetAreaPoints;
            for (int i = 0; i < points.Length; i++)
            {
                if (Physics.Raycast(points[i], -Vector3.up, out RaycastHit hit, Mathf.Infinity, layerMask,
                        QueryTriggerInteraction.Ignore))
                    spawnLocation.SetAreaPointPosition(i, hit.point + Vector3.up * AboveFloorDistance);
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
#endif
}