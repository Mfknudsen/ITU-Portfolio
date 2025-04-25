using System.Runtime.InteropServices;
using Runtime.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.AI.EntityBuffers
{
    public struct NavTriangleBufferElement : IBufferElementData
    {
        public readonly int ID;

        public readonly int A, B, C;

        public int NavGroup;

        public float3 AB, AC, BC;

        public readonly int NeighborOneA, NeighborOneB, NeighborTwoA, NeighborTwoB, NeighborThreeA, NeighborThreeB;

        public float3 Center;

        public readonly float MaxY, SquaredRadius;

        [MarshalAs(UnmanagedType.U1)] public readonly bool ABEdge, BCEdge, ACEdge;
        [MarshalAs(UnmanagedType.U1)] public bool WasUpdated;

        public readonly float NeighborOneWidth2D, NeighborTwoWidth2D, NeighborThreeWidth2D;

        public readonly int NeighborOneId, NeighborTwoId, NeighborThreeId;

        public float2 MinBound, MaxBound;

        public NavTriangleBufferElement(int id, int a, int b, int c, int navGroup, float3 ab, float3 ac, float3 bc,
            float maxY,
            float neighborOneWidth2D, float neighborTwoWidth2D, float neighborThreeWidth2D, bool abEdge,
            bool bcEdge, bool acEdge, DynamicBuffer<VertBufferElement> verts, int neighborOneId, int neighborTwoId,
            int neighborThreeId,
            int[] neighborOneShared, int[] neighborTwoShared, int[] neighborThreeShared)
        {
            this.ID = id;
            this.A = a;
            this.B = b;
            this.C = c;
            this.NavGroup = navGroup;
            this.AB = ab;
            this.AC = ac;
            this.BC = bc;
            this.MaxY = maxY;
            this.ABEdge = abEdge;
            this.BCEdge = bcEdge;
            this.ACEdge = acEdge;
            this.NeighborOneWidth2D = neighborOneWidth2D;
            this.NeighborTwoWidth2D = neighborTwoWidth2D;
            this.NeighborThreeWidth2D = neighborThreeWidth2D;
            this.NeighborOneId = neighborOneId;
            this.NeighborTwoId = neighborTwoId;
            this.NeighborThreeId = neighborThreeId;
            this.WasUpdated = true;

            this.NeighborOneA = neighborOneShared[0];
            this.NeighborOneB = neighborOneShared[1];
            this.NeighborTwoA = neighborTwoShared[0];
            this.NeighborTwoB = neighborTwoShared[1];
            this.NeighborThreeA = neighborThreeShared[0];
            this.NeighborThreeB = neighborThreeShared[1];

            this.Center = (verts[a].Position + verts[b].Position +
                           verts[c].Position) / 3f;
            this.SquaredRadius =
                Mathf.Max(
                    this.Center.QuickSquareDistance(verts[a].Position),
                    Mathf.Max(
                        this.Center.QuickSquareDistance(verts[b].Position)),
                    this.Center.QuickSquareDistance(verts[c].Position));

            this.MinBound = new float2(
                Mathf.Min(verts[a].Position.x, Mathf.Min(verts[b].Position.x, verts[c].Position.x),
                    Mathf.Min(verts[a].Position.z, Mathf.Min(verts[b].Position.z, verts[c].Position.z))));
            this.MaxBound = new float2(
                Mathf.Max(verts[a].Position.x, Mathf.Max(verts[b].Position.x, verts[c].Position.x),
                    Mathf.Max(verts[a].Position.z, Mathf.Max(verts[b].Position.z, verts[c].Position.z))));
        }

        public readonly void Vertices(ref NativeArray<int> reuseArray)
        {
            reuseArray[0] = this.A;
            reuseArray[1] = this.B;
            reuseArray[2] = this.C;
        }

        public readonly int EdgeCount()
        {
            int result = 0;

            if (this.ABEdge)
                result++;
            if (this.BCEdge)
                result++;
            if (this.ACEdge)
                result++;

            return result;
        }
    }
}