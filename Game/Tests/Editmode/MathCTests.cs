#region Libraries

using NUnit.Framework;
using Runtime.Core;
using UnityEngine;

#endregion

namespace Tests.Editmode
{
    public class MathCTests
    {
        [Test]
        public void LineIntersect2DWithTolerance()
        {
            Vector2 lineAStart = new Vector2(0, 0),
                lineAEnd = new Vector2(1, 1),
                lineBStart = new Vector2(0, 1),
                lineBEnd = new Vector2(1, 0),
                lineCStart = new Vector2(0, 0.5f),
                lineCEnd = new Vector2(.9f, 0.5f),
                lineDStart = new Vector2(1, 0),
                lineDEnd = new Vector2(1, 1);

            Assert.IsTrue(
                MathC.LineIntersect2DWithTolerance(lineAStart, lineAEnd, lineBStart, lineBEnd, .1f));
            Assert.IsFalse(
                MathC.LineIntersect2DWithTolerance(lineCStart, lineCEnd, lineDStart, lineDEnd, 0));
            Assert.IsTrue(
                MathC.LineIntersect2DWithTolerance(lineCStart, lineCEnd, lineDStart, lineDEnd, .1f));
        }

        [Test]
        public void LineIntersect2D()
        {
            Vector2 lineAStart = new Vector2(0, 0),
                lineAEnd = new Vector2(1, 1),
                lineBStart = new Vector2(0, 1),
                lineBEnd = new Vector2(1, 0),
                lineCStart = new Vector2(0, 0.5f),
                lineCEnd = new Vector2(.9f, 0.5f),
                lineDStart = new Vector2(1, 0),
                lineDEnd = new Vector2(1, 1);

            Assert.IsTrue(
                MathC.LineIntersect2D(lineAStart, lineAEnd, lineBStart, lineBEnd));
            Assert.IsFalse(
                MathC.LineIntersect2D(lineCStart, lineCEnd, lineDStart, lineDEnd));
            lineCEnd += new Vector2(.1f, 0);
            Assert.IsTrue(
                MathC.LineIntersect2D(lineCStart, lineCEnd, lineDStart, lineDEnd));
        }

        [Test]
        public void LerpPosition()
        {
        }

        [Test]
        public void PointWithinTriangle2DWithTolerance()
        {
        }

        [Test]
        public void PointWithinTriangle2D()
        {
        }

        [Test]
        public void TriangleIntersect2D()
        {
        }

        [Test]
        public void ClosetPointOnLine()
        {
        }

        [Test]
        public void ClosestPointValue()
        {
        }

        [Test]
        public void IsPointLeftToVector()
        {
            Vector2 pointLeft = Vector2.left, pointRight = Vector2.right, vector = Vector2.up;

            Assert.IsTrue(MathC.IsPointLeftToVector(Vector2.zero, vector, pointLeft));
            Assert.IsTrue(!MathC.IsPointLeftToVector(Vector2.zero, vector, pointRight));
            Assert.IsTrue(!MathC.IsPointLeftToVector(Vector2.zero, vector, vector));
        }

        [Test]
        public void QuickCircleIntersectCircleArea()
        {
        }

        [Test]
        public void ClosestPointLineIntersectCircle()
        {
        }

        [Test]
        public void RotateBy()
        {
        }
    }
}