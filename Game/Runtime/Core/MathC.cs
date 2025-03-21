#region Libraries

using Unity.Mathematics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

#endregion

namespace Runtime.Core
{
    public static class MathC
    {
        public static bool LineIntersect2DWithTolerance(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2,
            float tolerance = .001f)
        {
            if (tolerance == 0)
                return LineIntersect2D(start1, end1, start2, end2);

            //Line1
            float a1 = end1.y - start1.y;
            float b1 = start1.x - end1.x;
            float c1 = a1 * start1.x + b1 * start1.y;

            //Line2
            float a2 = end2.y - start2.y;
            float b2 = start2.x - end2.x;
            float c2 = a2 * start2.x + b2 * start2.y;

            float denominator = a1 * b2 - a2 * b1;

            if (denominator == 0)
                return false;

            float x1 = end1.x - start1.x,
                y1 = end1.y - start1.y,
                x2 = end2.x - start2.x,
                y2 = end2.y - start2.y;

            //Tolerance
            Vector2 offset0 = new Vector2(
                    x1 == 0 ? 0 : (Mathf.Abs(x1 + tolerance * 2f) / Mathf.Abs(x1) - 1f) / 2f,
                    y1 == 0 ? 0 : (Mathf.Abs(y1 + tolerance * 2f) / Mathf.Abs(y1) - 1f) / 2f) * 1.0001f,
                offset1 = new Vector2(
                    x2 == 0 ? 0 : (Mathf.Abs(x2 + tolerance * 2f) / Mathf.Abs(x2) - 1f) / 2f,
                    y2 == 0 ? 0 : (Mathf.Abs(y2 + tolerance * 2f) / Mathf.Abs(y2) - 1f) / 2f) * 1.0001f;

            Vector2 intersect = new Vector2((b2 * c1 - b1 * c2) / denominator, (a1 * c2 - a2 * c1) / denominator),
                r0 = new Vector2(
                    x1 == 0 ? 0 : (intersect.x - start1.x) / x1,
                    y1 == 0 ? 0 : (intersect.y - start1.y) / y1),
                r1 = new Vector2(
                    x2 == 0 ? 0 : (intersect.x - start2.x) / x2,
                    y2 == 0 ? 0 : (intersect.y - start2.y) / y2);

            return r0.x >= -offset0.x && r0.x <= 1f + offset0.x &&
                   r0.y >= -offset0.y && r0.y <= 1f + offset0.y &&
                   r1.x >= -offset1.x && r1.x <= 1f + offset1.x &&
                   r1.y >= -offset1.y && r1.y <= 1f + offset1.y;
        }

        public static bool LineIntersect2D(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2)
        {
            //Line1
            float a1 = end1.y - start1.y;
            float b1 = start1.x - end1.x;
            float c1 = a1 * start1.x + b1 * start1.y;

            //Line2
            float a2 = end2.y - start2.y;
            float b2 = start2.x - end2.x;
            float c2 = a2 * start2.x + b2 * start2.y;

            float denominator = a1 * b2 - a2 * b1;

            if (denominator == 0)
                return false;

            float x1 = end1.x - start1.x,
                x2 = end2.x - start2.x,
                y1 = end1.y - start1.y,
                y2 = end2.y - start2.y;

            Vector2 intersect = new Vector2((b2 * c1 - b1 * c2) / denominator, (a1 * c2 - a2 * c1) / denominator),
                r0 = new Vector2(
                    x1 == 0 ? 0 : (intersect.x - start1.x) / x1,
                    y1 == 0 ? 0 : (intersect.y - start1.y) / y1),
                r1 = new Vector2(
                    x2 == 0 ? 0 : (intersect.x - start2.x) / x2,
                    y2 == 0 ? 0 : (intersect.y - start2.y) / y2);

            return r0.x is >= 0 and <= 1 && r0.y is >= 0 and <= 1 && r1.x is >= 0 and <= 1 && r1.y is >= 0 and <= 1;
        }

        public static Vector3 CurveLerpPosition(AnimationCurve curve, float time, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float curveTime = curve.Evaluate(time);

            float u = 1 - curveTime;
            float tSquared = curveTime * curveTime;
            float uSquared = u * u;
            Vector3 result = uSquared * p0;
            result += 2 * u * curveTime * p1;
            result += tSquared * p2;

            return result;
        }

        public static bool PointWithinTriangle2DWithTolerance(Vector2 point, Vector2 a, Vector2 b, Vector2 c,
            float tolerance = .001f)
        {
            float s1 = c.y - a.y + 0.0001f;
            float s2 = c.x - a.x;
            float s3 = b.y - a.y;
            float s4 = point.y - a.y;

            float w1 = (a.x * s1 + s4 * s2 - point.x * s1) / (s3 * s2 - (b.x - a.x + 0.0001f) * s1);
            float w2 = (s4 - w1 * s3) / s1;
            return w1 >= tolerance && w2 >= tolerance && w1 + w2 <= 1f - tolerance;
        }

        //https://www.youtube.com/watch?v=HYAgJN3x4GA
        public static bool PointWithinTriangle2D(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
        {
            float s1 = c.y - a.y + 0.0001f;
            float s2 = c.x - a.x;
            float s3 = b.y - a.y;
            float s4 = point.y - a.y;

            float w1 = (a.x * s1 + s4 * s2 - point.x * s1) / (s3 * s2 - (b.x - a.x + 0.0001f) * s1);
            float w2 = (s4 - w1 * s3) / s1;
            return w1 >= 0 && w2 >= 0 && w1 + w2 <= 1;
        }

        //https://www.youtube.com/watch?v=HYAgJN3x4GA
        public static bool PointWithinTriangle2D(float2 point, float2 a, float2 b, float2 c)
        {
            float s1 = c.y - a.y + 0.0001f;
            float s2 = c.x - a.x;
            float s3 = b.y - a.y;
            float s4 = point.y - a.y;

            float w1 = (a.x * s1 + s4 * s2 - point.x * s1) / (s3 * s2 - (b.x - a.x + 0.0001f) * s1);
            float w2 = (s4 - w1 * s3) / s1;
            return w1 >= 0 && w2 >= 0 && w1 + w2 <= 1;
        }

        //https://www.youtube.com/watch?v=HYAgJN3x4GA
        public static bool PointWithinTriangle2D(Vector2 point, Vector2 a, Vector2 b, Vector2 c, out float w1,
            out float w2)
        {
            float s1 = c.y - a.y + 0.0001f;
            float s2 = c.x - a.x;
            float s3 = b.y - a.y;
            float s4 = point.y - a.y;

            w1 = (a.x * s1 + s4 * s2 - point.x * s1) / (s3 * s2 - (b.x - a.x + 0.0001f) * s1);
            w2 = (s4 - w1 * s3) / s1;
            return w1 >= 0 && w2 >= 0 && w1 + w2 <= 1;
        }

        public static bool PointWithinTriangle2D(float2 point, float2 a, float2 b, float2 c, out float w1,
            out float w2)
        {
            float s1 = c.y - a.y + 0.0001f;
            float s2 = c.x - a.x;
            float s3 = b.y - a.y;
            float s4 = point.y - a.y;

            w1 = (a.x * s1 + s4 * s2 - point.x * s1) / (s3 * s2 - (b.x - a.x + 0.0001f) * s1);
            w2 = (s4 - w1 * s3) / s1;
            return w1 >= 0 && w2 >= 0 && w1 + w2 <= 1;
        }

        public static bool TriangleIntersect2D(Vector2 a1, Vector2 a2, Vector2 a3, Vector2 b1, Vector2 b2, Vector2 b3)
        {
            return LineIntersect2DWithTolerance(a1, a2, b1, b2) ||
                   LineIntersect2DWithTolerance(a1, a3, b1, b2) ||
                   LineIntersect2DWithTolerance(a2, a3, b1, b2) ||
                   LineIntersect2DWithTolerance(a1, a2, b1, b3) ||
                   LineIntersect2DWithTolerance(a1, a3, b1, b3) ||
                   LineIntersect2DWithTolerance(a2, a3, b1, b3) ||
                   LineIntersect2DWithTolerance(a1, a2, b2, b3) ||
                   LineIntersect2DWithTolerance(a1, a3, b2, b3) ||
                   LineIntersect2DWithTolerance(a2, a3, b2, b3);
        }

        public static Vector2 ClosetPointOnLine(Vector2 point, Vector2 start, Vector2 end)
        {
            //Get heading
            Vector2 heading = end - start;
            float magnitudeMax = heading.magnitude;
            heading.Normalize();

            //Do projection from the point but clamp it
            Vector2 lhs = point - start;
            float dotP = Vector2.Dot(lhs, heading);
            dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);

            return start + heading * dotP;
        }

        public static Vector3 ClosetPointOnLine(Vector3 point, Vector3 start, Vector3 end)
        {
            //Get heading
            Vector3 heading = end - start;
            float magnitudeMax = heading.magnitude;
            heading.Normalize();

            //Do projection from the point but clamp it
            Vector3 lhs = point - start;
            float dotP = Vector3.Dot(lhs, heading);
            dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);

            return start + heading * dotP;
        }

        public static float ClosestPointValue(Vector2 point, Vector2 start, Vector2 end)
        {
            //Get heading
            Vector2 heading = end - start;
            float magnitudeMax = heading.magnitude;
            heading.Normalize();

            //Do projection from the point but clamp it
            Vector2 lhs = point - start;
            float dotP = Vector2.Dot(lhs, heading);
            return Mathf.Clamp(dotP, 0f, magnitudeMax);
        }

        public static bool IsPointLeftToVector(Vector2 lineA, Vector2 lineB, Vector2 point)
        {
            float r = (lineB.x - lineA.x) * (point.y - lineA.y) -
                      (lineB.y - lineA.y) * (point.x - lineA.x);

            return r > 0;
        }

        public static bool IsPointLeftToVector(float3 lineA, float3 lineB, float3 point)
        {
            return (lineB.x - lineA.x) * (point.z - lineA.z) -
                (lineB.z - lineA.z) * (point.x - lineA.x) > 0;
        }

        public static float QuickCircleIntersectCircleArea(Vector3 center1, Vector3 center2, float radius1,
            float radius2, float height1, float height2)
        {
            if (center1.y > center2.y + height2 || center2.y > center1.y + height1)
                return 0;

            float squaredRadius1 = radius1.Squared(),
                squaredRadius2 = radius2.Squared();

            float c = Mathf.Sqrt((center2.x - center1.x) * (center2.x - center1.x) +
                                 (center2.z - center1.z) * (center2.z - center1.z));

            float phi = Mathf.Acos((squaredRadius1 + c * c - squaredRadius2) / (2 * radius1 * c)) * 2;
            float theta = Mathf.Acos((squaredRadius2 + c * c - squaredRadius1) / (2 * radius2 * c)) * 2;

            float area1 = 0.5f * theta * squaredRadius2 - 0.5f * squaredRadius2 * Mathf.Sin(theta);
            float area2 = 0.5f * phi * squaredRadius1 - 0.5f * squaredRadius1 * Mathf.Sin(phi);

            return (area1 + area2) * Mathf.Abs(height1 - height2);
        }

        public static bool ClosestPointLineIntersectCircle(Vector2 lineStart, Vector2 lineEnd, Vector2 circlePoint,
            float circleRadius, out Vector2 hit)
        {
            float dx = lineEnd.x - lineStart.x;
            float dy = lineEnd.y - lineStart.y;

            float a = dx * dx + dy * dy;
            float b = 2 * (dx * (lineStart.x - circlePoint.x) + dy * (lineStart.y - circlePoint.y));
            float c = (lineStart.x - circlePoint.x) * (lineStart.x - circlePoint.x) +
                      (lineStart.y - circlePoint.y) * (lineStart.y - circlePoint.y) -
                      circleRadius * circleRadius;

            float det = b * b - 4f * a * c;
            float twoA = a * 2f;

            if (a <= 0.0000001f || det < 0f)
            {
                // No real solutions.
                hit = Vector2.zero;
                return false;
            }

            if (det == 0)
            {
                // One solution.
                float t = -b / twoA;
                hit = new Vector2(lineStart.x + t * dx, lineStart.y + t * dy);
                return true;
            }

            // Two solutions.
            float sqrtDet = Mathf.Sqrt(det);
            float tOne = (-b + sqrtDet) / twoA;
            Vector2 pointOne = new Vector2(lineStart.x + tOne * dx, lineStart.y + tOne * dy);
            float tTwo = (-b - sqrtDet) / twoA;
            Vector2 pointTwo = new Vector2(lineStart.x + tTwo * dx, lineStart.y + tTwo * dy);

            hit = (lineStart - pointOne).sqrMagnitude < (lineStart - pointTwo).sqrMagnitude ? pointOne : pointTwo;

            return true;
        }

        public static Vector2 RotateBy(Vector2 v, float angle)
        {
            float dirAngle = Mathf.Atan2(v.y, v.x);

            dirAngle *= 180f / Mathf.PI;

            float newAngle = -(dirAngle + angle) * Mathf.PI / 180f;

            Vector2 newDir = new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));

            return newDir;
        }
    }
}