#region Packages

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Runtime.Core.CorePath
{
    [System.Serializable]
    public class Path
    {
        [SerializeField, HideInInspector] private List<Vector3> points;
        [SerializeField, HideInInspector] private bool isClosed;
        [SerializeField, HideInInspector] private bool autoSetControlPoints;

        public Path(Vector3 centre)
        {
            this.points = new List<Vector3>
            {
                centre + Vector3.left,
                centre + (Vector3.left + Vector3.up) * .5f,
                centre + (Vector3.right + Vector3.down) * .5f,
                centre + Vector3.right
            };
        }

        public Vector3 this[int i] => this.points[i];

        public bool IsClosed
        {
            get => this.isClosed;
            set
            {
                if (this.isClosed != value)
                {
                    this.isClosed = value;

                    if (this.isClosed)
                    {
                        this.points.Add(this.points[this.points.Count - 1] * 2 - this.points[this.points.Count - 2]);
                        this.points.Add(this.points[0] * 2 - this.points[1]);
                        if (this.autoSetControlPoints)
                        {
                            this.AutoSetAnchorControlPoints(0);
                            this.AutoSetAnchorControlPoints(this.points.Count - 3);
                        }
                    }
                    else
                    {
                        this.points.RemoveRange(this.points.Count - 2, 2);
                        if (this.autoSetControlPoints) this.AutoSetStartAndEndControls();
                    }
                }
            }
        }

        public bool AutoSetControlPoints
        {
            get => this.autoSetControlPoints;
            set
            {
                if (this.autoSetControlPoints != value)
                {
                    this.autoSetControlPoints = value;
                    if (this.autoSetControlPoints)
                    {
                        this.AutoSetAllControlPoints();
                    }
                }
            }
        }

        public int NumPoints => this.points.Count;

        public int NumSegments => this.points.Count / 3;

        public void AddSegment(Vector3 anchorPos)
        {
            this.points.Add(this.points[this.points.Count - 1] * 2 - this.points[this.points.Count - 2]);
            this.points.Add((this.points[this.points.Count - 1] + anchorPos) * .5f);
            this.points.Add(anchorPos);

            if (this.autoSetControlPoints)
            {
                this.AutoSetAllAffectedControlPoints(this.points.Count - 1);
            }
        }

        public void SplitSegment(Vector3 anchorPos, int segmentIndex)
        {
            this.points.InsertRange(segmentIndex * 3 + 2, new[] { Vector3.zero, anchorPos, Vector3.zero });
            if (this.autoSetControlPoints)
                this.AutoSetAllAffectedControlPoints(segmentIndex * 3 + 3);
            else
                this.AutoSetAnchorControlPoints(segmentIndex * 3 + 3);
        }

        public void DeleteSegment(int anchorIndex)
        {
            if (this.NumSegments > 2 || !this.isClosed && this.NumSegments > 1)
            {
                if (anchorIndex == 0)
                {
                    if (this.isClosed) this.points[this.points.Count - 1] = this.points[2];

                    this.points.RemoveRange(0, 3);
                }
                else if (anchorIndex == this.points.Count - 1 && !this.isClosed)
                    this.points.RemoveRange(anchorIndex - 2, 3);
                else
                    this.points.RemoveRange(anchorIndex - 1, 3);
            }
        }

        public Vector3[] GetPointsInSegment(int i) => new[]
            {
                this.points[i * 3], this.points[i * 3 + 1], this.points[i * 3 + 2], this.points[this.LoopIndex(i * 3 + 3)] };

        public void MovePoint(int i, Vector3 pos)
        {
            Vector3 deltaMove = pos - this.points[i];

            if (i % 3 == 0 || !this.autoSetControlPoints)
            {
                this.points[i] = pos;

                if (this.autoSetControlPoints)
                    this.AutoSetAllAffectedControlPoints(i);
                else
                {
                    if (i % 3 == 0)
                    {
                        if (i + 1 < this.points.Count || this.isClosed) this.points[this.LoopIndex(i + 1)] += deltaMove;

                        if (i - 1 >= 0 || this.isClosed) this.points[this.LoopIndex(i - 1)] += deltaMove;
                    }
                    else
                    {
                        bool nextPointIsAnchor = (i + 1) % 3 == 0;
                        int correspondingControlIndex = (nextPointIsAnchor) ? i + 2 : i - 2;
                        int anchorIndex = (nextPointIsAnchor) ? i + 1 : i - 1;

                        if (correspondingControlIndex >= 0 && correspondingControlIndex < this.points.Count || this.isClosed)
                        {
                            float dst = (this.points[this.LoopIndex(anchorIndex)] - this.points[this.LoopIndex(correspondingControlIndex)])
                                .magnitude;
                            Vector3 dir = (this.points[this.LoopIndex(anchorIndex)] - pos).normalized;
                            this.points[this.LoopIndex(correspondingControlIndex)] = this.points[this.LoopIndex(anchorIndex)] + dir * dst;
                        }
                    }
                }
            }
        }

        public Vector3[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1)
        {
            List<Vector3> evenlySpacedPoints = new List<Vector3>();
            evenlySpacedPoints.Add(this.points[0]);
            Vector3 previousPoint = this.points[0];
            float dstSinceLastEvenPoint = 0;

            for (int segmentIndex = 0; segmentIndex < this.NumSegments; segmentIndex++)
            {
                Vector3[] p = this.GetPointsInSegment(segmentIndex);
                float controlNetLength = Vector3.Distance(p[0], p[1]) + Vector3.Distance(p[1], p[2]) +
                                         Vector3.Distance(p[2], p[3]);
                float estimatedCurveLength = Vector3.Distance(p[0], p[3]) + controlNetLength / 2f;
                int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
                float t = 0;
                while (t <= 1)
                {
                    t += 1f / divisions;
                    Vector3 pointOnCurve = Bezier.EvaluateCubic(p[0], p[1], p[2], p[3], t);
                    dstSinceLastEvenPoint += Vector3.Distance(previousPoint, pointOnCurve);

                    while (dstSinceLastEvenPoint >= spacing)
                    {
                        float overshootDst = dstSinceLastEvenPoint - spacing;
                        Vector3 newEvenlySpacedPoint =
                            pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDst;
                        evenlySpacedPoints.Add(newEvenlySpacedPoint);
                        dstSinceLastEvenPoint = overshootDst;
                        previousPoint = newEvenlySpacedPoint;
                    }

                    previousPoint = pointOnCurve;
                }
            }

            return evenlySpacedPoints.ToArray();
        }


        private void AutoSetAllAffectedControlPoints(int updatedAnchorIndex)
        {
            for (int i = updatedAnchorIndex - 3; i <= updatedAnchorIndex + 3; i += 3)
            {
                if (i >= 0 && i < this.points.Count || this.isClosed) this.AutoSetAnchorControlPoints(this.LoopIndex(i));
            }

            this.AutoSetStartAndEndControls();
        }

        private void AutoSetAllControlPoints()
        {
            for (int i = 0; i < this.points.Count; i += 3) this.AutoSetAnchorControlPoints(i);

            this.AutoSetStartAndEndControls();
        }

        private void AutoSetAnchorControlPoints(int anchorIndex)
        {
            Vector3 anchorPos = this.points[anchorIndex];
            Vector3 dir = Vector3.zero;
            float[] neighbourDistances = new float[2];

            if (anchorIndex - 3 >= 0 || this.isClosed)
            {
                Vector3 offset = this.points[this.LoopIndex(anchorIndex - 3)] - anchorPos;
                dir += offset.normalized;
                neighbourDistances[0] = offset.magnitude;
            }

            if (anchorIndex + 3 >= 0 || this.isClosed)
            {
                Vector3 offset = this.points[this.LoopIndex(anchorIndex + 3)] - anchorPos;
                dir -= offset.normalized;
                neighbourDistances[1] = -offset.magnitude;
            }

            dir.Normalize();

            for (int i = 0; i < 2; i++)
            {
                int controlIndex = anchorIndex + i * 2 - 1;
                
                if (controlIndex >= 0 && controlIndex < this.points.Count || this.isClosed) this.points[this.LoopIndex(controlIndex)] = anchorPos + dir * (neighbourDistances[i] * .5f);
            }
        }

        private void AutoSetStartAndEndControls()
        {
            if (!this.isClosed)
            {
                this.points[1] = (this.points[0] + this.points[2]) * .5f;
                this.points[this.points.Count - 2] = (this.points[this.points.Count - 1] + this.points[this.points.Count - 3]) * .5f;
            }
        }

        private int LoopIndex(int i)
        {
            return (i + this.points.Count) % this.points.Count;
        }
    }
}