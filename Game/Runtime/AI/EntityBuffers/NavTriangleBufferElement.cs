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
        public int ID;

        public int A, B, C;

        public int NavGroup;

        public float2 Ab, Ac, BC;

        public int NeighborOneA, NeighborOneB, NeighborTwoA, NeighborTwoB, NeighborThreeA, NeighborThreeB;

        public float3 Center;

        public float MaxY, SquaredRadius;

        [MarshalAs(UnmanagedType.U1)] public bool AbEdge, BcEdge, AcEdge;
        [MarshalAs(UnmanagedType.U1)] public bool WasUpdated;

        public float NeighborOneWidth2D, NeighborTwoWidth2D, NeighborThreeWidth2D;

        public int NeighborOneId, NeighborTwoId, NeighborThreeId;

        public float2 minBound, maxBound;

        public NavTriangleBufferElement(int id, int a, int b, int c, int navGroup, float2 ab, float2 ac, float2 bc,
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
            this.Ab = ab;
            this.Ac = ac;
            this.BC = bc;
            this.MaxY = maxY;
            this.AbEdge = abEdge;
            this.BcEdge = bcEdge;
            this.AcEdge = acEdge;
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

            this.minBound = new float2(
                Mathf.Min(verts[a].Position.x, Mathf.Min(verts[b].Position.x, verts[c].Position.x),
                    Mathf.Min(verts[a].Position.z, Mathf.Min(verts[b].Position.z, verts[c].Position.z))));
            this.maxBound = new float2(
                Mathf.Max(verts[a].Position.x, Mathf.Max(verts[b].Position.x, verts[c].Position.x),
                    Mathf.Max(verts[a].Position.z, Mathf.Max(verts[b].Position.z, verts[c].Position.z))));
        }

        public readonly void Vertices(ref NativeArray<int> reuseArray)
        {
            reuseArray[0] = this.A;
            reuseArray[1] = this.B;
            reuseArray[2] = this.C;
        }
    }
}