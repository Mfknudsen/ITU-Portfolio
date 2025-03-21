using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.AI.EntityBuffers
{
    public struct VertBufferElement : IBufferElementData
    {
        public float3 Position;
        public int navGroup;
        public float RemappingXDir, RemappingZDir;
        [MarshalAs(UnmanagedType.U1)] public bool WasUpdated;

        public float Distance2D(VertBufferElement other)
        {
            return Vector3.Distance(this.Position, other.Position);
        }

        public float2 XZ()
        {
            return new float2(this.Position.x, this.Position.z);
        }

        public void Set(in float3 p)
        {
            if (math.all(this.Position == p))
                return;

            this.Position = p;

            this.WasUpdated = true;
        }
    }
}