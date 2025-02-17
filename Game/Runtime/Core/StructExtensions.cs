#region Libraries

using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

#endregion

namespace Runtime.Core
{
    public static class StructExtensions
    {
        #region int

        public static int RandomUniqueIndex(this int currentIndex, int listCount)
        {
            return currentIndex + (currentIndex + Random.Range(1, listCount - 1)) % listCount;
        }

        #endregion

        #region bool

        public static void Reverse(this ref bool b)
        {
            b = !b;
        }

        #endregion

        #region float

        public static float PercentageOf(this float check, float max)
        {
            return check / (max / 100);
        }

        public static float Clamp(this Vector2 bounds, float current)
        {
            return Mathf.Clamp(current, bounds.x, bounds.y);
        }

        public static float Clamp(this float current, float min, float max)
        {
            return Mathf.Clamp(current, min, max);
        }

        public static void RefClamp(this ref float current, float min, float max)
        {
            current = Mathf.Clamp(current, min, max);
        }

        public static float Squared(this float current)
        {
            return current * current;
        }

        public static float2 XZFloat(this float3 target)
        {
            return new float2(target.x, target.z);
        }

        #endregion

        #region float2

        public static float Distance(this float2 a)
        {
            return Mathf.Sqrt(Mathf.Pow(a.x, 2f) + Mathf.Pow(a.y, 2f));
        }

        public static float Distance(this float2 a, float2 b)
        {
            return Mathf.Sqrt(Mathf.Pow(b.x - a.x, 2f) + Mathf.Pow(b.y - a.y, 2f));
        }

        public static float2 Normalize(this float2 v)
        {
            float d = Mathf.Sqrt(v.x * v.x + v.y * v.y);
            return v / d;
        }

        public static float3 To3(this float2 v, float y)
        {
            return new float3(v.x, y, v.y);
        }

        #endregion

        #region float3

        public static float2 XZ(this float3 v)
        {
            return new float2(v.x, v.z);
        }

        public static Vector3 ToV(this float3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static float3 Normalize(this float3 v)
        {
            float m = Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
            return v / m;
        }

        public static float QuickSquareDistance(this float3 a, float3 b)
        {
            float3 v = a - b;
            return v.x * v.x + v.y * v.y + v.z * v.z;
        }

        #endregion

        #region Vector2

        public static System.Numerics.Vector2 ToNurmerics(this Vector2 target)
        {
            return new System.Numerics.Vector2(target.x, target.y);
        }

        public static Vector3 ToV3(this Vector2 t, float y)
        {
            return new Vector3(t.x, y, t.y);
        }

        public static float QuickSquareDistance(this Vector2 point1, Vector2 point2)
        {
            return (point1 - point2).sqrMagnitude;
        }

        public static Vector2 Cross(this Vector2 target)
        {
            return new Vector2(target.y, -target.x);
        }

        public static float QuickSquareRootMagnitude(this Vector2 target)
        {
            return Mathf.Sqrt(target.x.Squared() + target.y.Squared());
        }

        public static Vector2 FastNorm(this Vector2 target)
        {
            return target / target.QuickSquareRootMagnitude();
        }

        public static Vector2 Mul(this Vector2 a, Vector2 b)
        {
            return new Vector2(a.x * b.x, a.y * b.y);
        }

        #endregion

        #region Vector3

        public static float2 ToFloatXZ(this Vector3 target)
        {
            return new float2(target.x, target.z);
        }

        public static Vector2 XZ(this Vector3 target)
        {
            return new Vector2(target.x, target.z);
        }

        public static float QuickSquareDistance(this Vector3 point1, Vector3 point2)
        {
            return (point1 - point2).sqrMagnitude;
        }

        public static bool QuickDistanceLessThen(this Vector3 point1, Vector3 point2, float distance)
        {
            return QuickSquareDistance(point1, point2) < distance * distance;
        }

        public static float ShortDistancePointToLine(this Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 line = lineEnd - lineStart;
            Vector3 startToPoint = point - lineStart;

            float area = Vector3.Cross(line, startToPoint).magnitude;

            return area / line.magnitude;
        }

        public static float QuickSquareRootMagnitude(this Vector3 target)
        {
            return Mathf.Sqrt(target.x.Squared() + target.y.Squared() + target.z.Squared());
        }

        public static Vector3 FastNorm(this Vector3 target)
        {
            return target / target.QuickSquareRootMagnitude();
        }

        public static Vector3 Mul(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static Vector3 Mul(this Vector3 a, float x = 1, float y = 1, float z = 1)
        {
            return new Vector3(a.x * x, a.y * y, a.z * z);
        }

        #endregion

        #region Quaternion

        public static Vector3 ForwardFromRotation(this Quaternion quaternion)
        {
            return quaternion * Vector3.forward;
        }

        public static Vector3 UpFromRotation(this Quaternion quaternion)
        {
            return quaternion * Vector3.up;
        }

        public static Vector3 RightFromRotation(this Quaternion quaternion)
        {
            return quaternion * Vector3.right;
        }

        #endregion

        #region Array/List

        public static bool ContainsAny<T>(this T[] target, T[] other)
        {
            foreach (T item in target)
            foreach (T otherItem in other)
                if (item.Equals(otherItem))
                    return true;

            return false;
        }

        public static T RandomFrom<T>(this T[] target)
        {
            return target[Random.Range(0, target.Length)];
        }

        public static T RandomFrom<T>(this List<T> target)
        {
            return target[Random.Range(0, target.Count)];
        }

        public static T[] SharedBetween<T>(this T[] target, T[] other, int max = -1)
        {
            List<T> result = new List<T>();

            foreach (T a in target)
            {
                foreach (T b in other)
                {
                    if (a.Equals(b))
                    {
                        result.Add(a);
                        break;
                    }

                    if (result.Count == max)
                        break;
                }

                if (result.Count == max)
                    break;
            }

            return result.ToArray();
        }

        public static List<T> SharedBetween<T>(this List<T> target, List<T> other)
        {
            List<T> result = new List<T>();

            foreach (T a in target)
            foreach (T b in other)
                if (a.Equals(b))
                {
                    result.Add(a);
                    break;
                }

            return result.ToList();
        }

        public static int SharedBetweenCount<T>(this T[] target, T[] other, int max = -1)
        {
            int result = 0;

            foreach (T a in target)
            {
                foreach (T b in other)
                {
                    if (a.Equals(b))
                    {
                        result++;
                        break;
                    }

                    if (result == max)
                        break;
                }

                if (result == max)
                    break;
            }

            return result;
        }

        public static List<T> ReverseList<T>(this List<T> target)
        {
            if (target.Count == 0)
                return target;

            int a = 0, b = target.Count - 1;
            T temp;

            while (a != b && a < b)
            {
                temp = target[a];
                target[a] = target[b];
                target[b] = temp;

                a++;
                b--;
            }

            return target;
        }

        public static bool ValidIndex<T>(this T[,] array, int x, int y)
        {
            return x >= 0 && y >= 0 && x < array.GetLength(0) && y < array.GetLength(1);
        }

        #endregion
    }
}