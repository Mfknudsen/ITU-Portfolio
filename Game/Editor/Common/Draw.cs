#region Libraries

using UnityEngine;

#endregion

namespace Editor.Common
{
    public static class Draw
    {
        //https://dev-tut.com/2022/unity-draw-a-circle-part2/
        public static void DrawCircle(Vector3 position, float radius, Color color, int segments = 32)
        {
            if (radius <= 0 || segments <= 0)
                return;

            Vector3 lineStart, lineEnd;
            float angleStep = 360f / segments * Mathf.Deg2Rad;
            for (int i = 0; i < segments; i++)
            {
                // Line start is defined as starting angle of the current segment (i)
                lineStart = new Vector3(Mathf.Cos(angleStep * i), 0, Mathf.Sin(angleStep * i));

                // Line end is defined by the angle of the next segment (i+1)
                lineEnd = new Vector3(Mathf.Cos(angleStep * (i + 1)), 0, Mathf.Sin(angleStep * (i + 1)));

                // Results are multiplied so they match the desired radius
                lineStart *= radius;
                lineEnd *= radius;

                // Results are offset by the desired position/origin 
                lineStart += position;
                lineEnd += position;

                // Points are connected using DrawLine method and using the passed color
                Debug.DrawLine(lineStart, lineEnd, color);
            }
        }

        public static void DrawCylinder(Vector3 position, float height, float radius, Color color, int circleSegments = 32)
        {
            DrawCircle(position, radius, color, circleSegments);
            DrawCircle(position + Vector3.up * height, radius, color, circleSegments);

            Vector3 lineStart, lineEnd;
            float angleStep = 360f / 4 * Mathf.Deg2Rad;
            for (int i = 0; i < 4; i++)
            {
                lineStart = new Vector3(Mathf.Cos(angleStep * i), 0, Mathf.Sin(angleStep * i)) * radius + position;
                lineEnd = lineStart + Vector3.up * height;

                Debug.DrawLine(lineStart, lineEnd, color);
            }
        }
    }
}