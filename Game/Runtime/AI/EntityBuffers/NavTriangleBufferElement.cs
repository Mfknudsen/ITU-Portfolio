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

        public float2 Ab, Ac, BC;

        public float3 Center;

        public float MaxY, SquaredRadius;

        [MarshalAs(UnmanagedType.U1)]
        public bool AbEdge, BcEdge, AcEdge;
        [MarshalAs(UnmanagedType.U1)]
        public bool CellNeedsUpdate;

        public float NeighborOneWidth, NeighborTwoWidth, NeighborThreeWidth;

        public int NeighborOne, NeighborTwo, NeighborThree;

        public float2 minBound, maxBound;

        public NavTriangleBufferElement(int id, int a, int b, int c, float2 ab, float2 ac, float2 bc, float maxY,
            float neighborOneWidth, float neighborTwoWidth, float neighborThreeWidth, bool abEdge,
            bool bcEdge, bool acEdge, DynamicBuffer<VertXZBufferElement> simpleVerts,
            DynamicBuffer<VertYBufferElement> vertsY, int neighborOne = -1,
            int neighborTwo = -1, int neighborThree = -1)
        {
            this.ID = id;
            this.A = a;
            this.B = b;
            this.C = c;
            this.Ab = ab;
            this.Ac = ac;
            this.BC = bc;
            this.MaxY = maxY;
            this.AbEdge = abEdge;
            this.BcEdge = bcEdge;
            this.AcEdge = acEdge;
            this.NeighborOneWidth = neighborOneWidth;
            this.NeighborTwoWidth = neighborTwoWidth;
            this.NeighborThreeWidth = neighborThreeWidth;
            this.NeighborOne = neighborOne;
            this.NeighborTwo = neighborTwo;
            this.NeighborThree = neighborThree;
            this.CellNeedsUpdate = true;
            this.Center = (simpleVerts[a].ToV3(vertsY[a]) + simpleVerts[b].ToV3(vertsY[b]) +
                           simpleVerts[c].ToV3(vertsY[c])) / 3f;
            this.SquaredRadius =
                Mathf.Max(
                    this.Center.QuickSquareDistance(simpleVerts[a].ToV3(vertsY[a])),
                    Mathf.Max(
                        this.Center.QuickSquareDistance(simpleVerts[b].ToV3(vertsY[b])),
                        this.Center.QuickSquareDistance(simpleVerts[c].ToV3(vertsY[c]))));

            this.minBound = new float2(
                Mathf.Min(simpleVerts[a].X, Mathf.Min(simpleVerts[b].X, simpleVerts[c].X),
                    Mathf.Min(simpleVerts[a].Z, Mathf.Min(simpleVerts[b].Z, simpleVerts[c].Z))));
            this.maxBound = new float2(
                Mathf.Max(simpleVerts[a].X, Mathf.Max(simpleVerts[b].X, simpleVerts[c].X),
                    Mathf.Max(simpleVerts[a].Z, Mathf.Max(simpleVerts[b].Z, simpleVerts[c].Z))));
        }

        public readonly NativeArray<int> Vertices()
        {
            return new NativeArray<int>(3, Allocator.Temp)
            {
                [0] = this.A,
                [1] = this.B,
                [2] = this.C
            };
        }
    }
}