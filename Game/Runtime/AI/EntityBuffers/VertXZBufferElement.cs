using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.AI.EntityBuffers
{
    public struct VertXZBufferElement : IBufferElementData
    {
        public float X, Z;
        public float RemappingXDir, RemappingZDir;

        public float QuickSquareDistance(VertYBufferElement vertY, float3 point)
        {
            float x = this.X - point.x, y = vertY.Y - point.y, z = this.Z - point.z;
            return x * x + y * y + z * z;
        }

        public float Distance(VertXZBufferElement other)
        {
            return Mathf.Sqrt(Mathf.Pow(this.X - other.X, 2f) + Mathf.Pow(this.Z - other.X, 2f));
        }

        public static float2 operator -(VertXZBufferElement a, VertXZBufferElement b)
        {
            return new float2(a.X - b.X, a.Z - b.Z);
        }

        public float2 XZ()
        {
            return new float2(this.X, this.Z);
        }

        public float3 ToV3(VertYBufferElement vert)
        {
            return new float3(this.X, vert.Y, this.Z);
        }
    }
}