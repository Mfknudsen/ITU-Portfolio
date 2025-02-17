#region Libraries

using Runtime.Core.CorePath;
using UnityEditor;
using UnityEngine;

#endregion

namespace Editor
{
    [CustomEditor(typeof(PathCreator))]
    public class PathEditor : UnityEditor.Editor
    {
        private PathCreator creator;

        private Path path => this.creator.path;

        private const float SegmentSelectDistanceThreshold = .1f;
        private int selectedSegmentIndex = -1;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            if (GUILayout.Button("Create new"))
            {
                Undo.RecordObject(this.creator, "Create new");
                this.creator.CreatePath();
            }

            bool isClosed = GUILayout.Toggle(this.path.IsClosed, "Closed");
            if (isClosed != this.path.IsClosed)
            {
                Undo.RecordObject(this.creator, "Toggle closed");
                this.path.IsClosed = isClosed;
            }

            bool autoSetControlPoints = GUILayout.Toggle(this.path.AutoSetControlPoints, "Auto Set Control Points");
            if (autoSetControlPoints != this.path.AutoSetControlPoints)
            {
                Undo.RecordObject(this.creator, "Toggle auto set controls");
                this.path.AutoSetControlPoints = autoSetControlPoints;
            }

            if (EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();
        }

        private void OnSceneGUI()
        {
            this.Input();
            this.Draw();
        }

        private void Input()
        {
            Event guiEvent = Event.current;
            Vector3 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
            {
                if (this.selectedSegmentIndex != -1)
                {
                    Undo.RecordObject(this.creator, "Split segment");
                    this.path.SplitSegment(mousePos, this.selectedSegmentIndex);
                }
                else if (!this.path.IsClosed)
                {
                    Undo.RecordObject(this.creator, "Add segment");
                    this.path.AddSegment(mousePos);
                }
            }

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
            {
                float minDstToAnchor = this.creator.anchorDiameter * .5f;
                int closestAnchorIndex = -1;

                for (int i = 0; i < this.path.NumPoints; i += 3)
                {
                    float dst = Vector3.Distance(mousePos, this.path[i]);
                    if (dst < minDstToAnchor)
                    {
                        minDstToAnchor = dst;
                        closestAnchorIndex = i;
                    }
                }

                if (closestAnchorIndex != -1)
                {
                    Undo.RecordObject(this.creator, "Delete segment");
                    this.path.DeleteSegment(closestAnchorIndex);
                }
            }

            if (guiEvent.type == EventType.MouseMove)
            {
                float minDstToSegment = SegmentSelectDistanceThreshold;
                int newSelectedSegmentIndex = -1;

                for (int i = 0; i < this.path.NumSegments; i++)
                {
                    Vector3[] points = this.path.GetPointsInSegment(i);
                    float dst = HandleUtility.DistancePointBezier(mousePos, points[0], points[3], points[1], points[2]);
                    if (dst < minDstToSegment)
                    {
                        minDstToSegment = dst;
                        newSelectedSegmentIndex = i;
                    }
                }

                if (newSelectedSegmentIndex != this.selectedSegmentIndex)
                {
                    this.selectedSegmentIndex = newSelectedSegmentIndex;
                    HandleUtility.Repaint();
                }
            }

            HandleUtility.AddDefaultControl(0);
        }

        private void Draw()
        {
            for (int i = 0; i < this.path.NumSegments; i++)
            {
                Vector3[] points = this.path.GetPointsInSegment(i);
                if (this.creator.displayControlPoints)
                {
                    Handles.color = Color.black;
                    Handles.DrawLine(points[1], points[0]);
                    Handles.DrawLine(points[2], points[3]);
                }

                Color segmentCol = i == this.selectedSegmentIndex && Event.current.shift
                    ? this.creator.selectedSegmentCol
                    : this.creator.segmentCol;

                Handles.DrawBezier(points[0], points[3], points[1], points[2], segmentCol, null, 2);
            }

            for (int i = 0; i < this.path.NumPoints; i++)
            {
                if (i % 3 == 0 || this.creator.displayControlPoints)
                {
                    Handles.color = i % 3 == 0 ? this.creator.anchorCol : this.creator.controlCol;
                    float handleSize = i % 3 == 0 ? this.creator.anchorDiameter : this.creator.controlDiameter;
                    Quaternion fmh_148_75_638231264529077468 = Quaternion.identity;
                    Vector3 newPos = Handles.FreeMoveHandle(this.path[i], handleSize, Vector3.zero,
                        Handles.CylinderHandleCap);

                    if (this.path[i] != newPos)
                    {
                        Undo.RecordObject(this.creator, "Move point");
                        this.path.MovePoint(i, newPos);
                    }
                }
            }
        }

        private void OnEnable()
        {
            this.creator = (PathCreator)this.target;
            if (this.creator.path == null)
            {
                this.creator.CreatePath();
            }
        }
    }
}