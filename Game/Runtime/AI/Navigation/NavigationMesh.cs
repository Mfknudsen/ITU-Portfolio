#region Libraries

using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.Core;
using Runtime.Variables;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

#endregion

namespace Runtime.AI.Navigation
{
    public sealed class NavigationMesh : SerializedScriptableObject
    {
        #region Values

        [SerializeField] private Vector2[] vertices2D;
        [SerializeField] private float[] verticesY;

        [SerializeField] private NavTriangle[] triangles;
        [SerializeField] private int[] areaType;
        [SerializeField] private SDictionary<int, List<NavigationPointEntry>> navigationEntryPoints;

        /// <summary>
        ///     Index of vertex returns all NavTriangles containing the vertex id.
        /// </summary>
        [SerializeField] private List<TrianglesByVertexElement> triangleByVertexID;

        private NavigationPoint[] navigationPoints;

        public const float GroupDivision = 5f;

        [SerializeField] [ReadOnly] private float minFloorX,
            minFloorZ,
            maxFloorX,
            maxFloorZ;

        #endregion

        #region Getters

        public float[] GetVertY => this.verticesY;

        public Vector2[] SimpleVertices => this.vertices2D;

        public NavTriangle[] Triangles => this.triangles;

        public int[] Areas => this.areaType;

        public List<int> GetTrianglesByVertexID(int id)
        {
            return this.triangleByVertexID[id].GetList();
        }

        public float GetMinX()
        {
            return this.minFloorX;
        }

        public float GetMinZ()
        {
            return this.minFloorZ;
        }

        public float GetMaxX()
        {
            return this.maxFloorX;
        }

        public float GetMaxZ()
        {
            return this.maxFloorZ;
        }

        public float GetGroupDivisionSize()
        {
            return GroupDivision;
        }

        #endregion

        #region Setters

        public void SetValues(Vector3[] vertices, NavTriangle[] navTriangles, int[] areaTypes,
            Dictionary<int, List<NavigationPointEntry>> entryPoints)
        {
            this.triangles = new NavTriangle[navTriangles.Length];
            for (int i = 0; i < navTriangles.Length; i++)
                this.triangles[i] = navTriangles[i];

            this.areaType = areaTypes;
            this.navigationEntryPoints = entryPoints as SDictionary<int, List<NavigationPointEntry>>;

            this.vertices2D = new Vector2[vertices.Length];
            this.verticesY = new float[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                this.vertices2D[i] = new Vector2(vertices[i].x, vertices[i].z);
                this.verticesY[i] = vertices[i].y;
            }

            this.triangleByVertexID = new List<TrianglesByVertexElement>(vertices.Length);
            for (int i = 0; i < vertices.Length; i++)
                this.triangleByVertexID.Add(new TrianglesByVertexElement());

            navTriangles.ForEach(t =>
            {
                foreach (int triangleVertex in t.Vertices)
                    this.triangleByVertexID[triangleVertex].Add(t.ID);
            });

            this.minFloorX = this.maxFloorX = this.vertices2D[0].x;
            this.minFloorZ = this.maxFloorZ = this.vertices2D[0].y;

            foreach (Vector2 vertex in this.vertices2D)
            {
                this.minFloorX = Mathf.Min(this.minFloorX, vertex.x);
                this.minFloorZ = Mathf.Min(this.minFloorZ, vertex.y);

                this.maxFloorX = Mathf.Max(this.maxFloorX, vertex.x);
                this.maxFloorZ = Mathf.Max(this.maxFloorZ, vertex.y);
            }

            this.minFloorX -= GroupDivision;
            this.minFloorZ -= GroupDivision;
            this.maxFloorX += GroupDivision;
            this.maxFloorZ += GroupDivision;
        }

        public void SetVert(int index, Vector3 v)
        {
            this.vertices2D[index] = new Vector2(v.x, v.z);

            this.verticesY[index] = v.y;
        }

        #endregion

        #region Out

        public Vector3[] Vertices()
        {
            Vector3[] result = new Vector3[this.verticesY.Length];

            for (int i = 0; i < this.verticesY.Length; i++)
                result[i] = new Vector3(this.vertices2D[i].x, this.verticesY[i], this.vertices2D[i].y);

            return result;
        }

        public NavTriangle[] GetByVertexIndex(int index)
        {
            NavTriangle[] result = new NavTriangle[this.triangleByVertexID[index].Count];

            for (int i = 0; i < result.Length; i++)
                result[i] = this.triangles[this.triangleByVertexID[index].Get(i)];

            return result;
        }

        public int[] Indices => this.triangles.SelectMany(t => t.Vertices).ToArray();

        public int ClosestTriangleIndex(Vector3 p)
        {
            Vector2 agentXZ = p.XZ();

            foreach (NavTriangle navTriangle in this.Triangles)
            {
                if (!MathC.PointWithinTriangle2DWithTolerance(agentXZ,
                        this.SimpleVertices[navTriangle.GetA],
                        this.SimpleVertices[navTriangle.GetB],
                        this.SimpleVertices[navTriangle.GetC]))
                    continue;

                return navTriangle.ID;
            }

            float dist = (this.SimpleVertices[0] - agentXZ).sqrMagnitude;
            int selected = 0;

            for (int i = 1; i < this.SimpleVertices.Length; i++)
            {
                float newDist = (this.SimpleVertices[i] - agentXZ).sqrMagnitude;
                if (newDist > dist)
                    continue;

                dist = newDist;
                selected = i;
            }

            List<int> trianglesByVertexID = this.GetTrianglesByVertexID(selected);
            foreach (int navTriangleID in trianglesByVertexID)
            {
                NavTriangle navTriangle = this.Triangles[navTriangleID];
                if (!MathC.PointWithinTriangle2DWithTolerance(agentXZ,
                        this.SimpleVertices[navTriangle.GetA],
                        this.SimpleVertices[navTriangle.GetB],
                        this.SimpleVertices[navTriangle.GetC]))
                    continue;

                return navTriangle.ID;
            }

            return trianglesByVertexID.RandomFrom();
        }

        public int ClosestTriangleIndex(Vector2 p)
        {
            for (int i = 0; i < this.triangles.Length; i++)
            {
                int[] ids = this.triangles[i].Vertices;
                if (MathC.PointWithinTriangle2D(p,
                        this.SimpleVertices[ids[0]],
                        this.SimpleVertices[ids[1]],
                        this.SimpleVertices[ids[2]]))
                    return this.triangles[i].ID;
            }

            return this.triangles.RandomFrom().ID;
        }

        public Vector3 VertByIndex(int i)
        {
            return new Vector3(this.vertices2D[i].x, this.verticesY[i], this.vertices2D[i].y);
        }

        #endregion
    }

    [Serializable]
    public struct NavTriangle
    {
        #region Values

        [SerializeField] private int id;

        [SerializeField] private int a, b, c;

        [SerializeField] private int area;

        [SerializeField] private float maxY;

        [SerializeField] private bool abEdge, bcEdge, acEdge;

        [SerializeField] private List<int> neighborIDs;
        [SerializeField] private List<float> widthDistanceBetweenNeighbor;
        [SerializeField] private List<int> navPointIDs;

        [SerializeField] private Vector2 ab, bc, ac;

        #endregion

        #region Build In States

        public NavTriangle(int id, int a, int b, int c, int area,
            params Vector3[] verts)
        {
            this.id = id;
            this.a = a;
            this.b = b;
            this.c = c;

            this.acEdge = false;
            this.bcEdge = false;
            this.abEdge = false;

            this.maxY = Mathf.Max(Mathf.Max(verts[0].y, verts[1].y), verts[2].y);

            this.area = area;

            this.neighborIDs = new List<int>();
            this.navPointIDs = new List<int>();
            this.widthDistanceBetweenNeighbor = new List<float>();

            this.ab = verts[1].XZ() - verts[0].XZ();
            this.bc = verts[2].XZ() - verts[1].XZ();
            this.ac = verts[2].XZ() - verts[0].XZ();
        }

        #endregion

        #region Getters

        public readonly int GetA => this.a;

        public readonly int GetB => this.b;

        public readonly int GetC => this.c;

        public readonly int[] Vertices => new int[] { this.a, this.b, this.c };

        public readonly List<int> Neighbors => this.neighborIDs;

        public readonly List<int> NavPoints => this.navPointIDs;

        public readonly List<float> Widths => this.widthDistanceBetweenNeighbor;

        public readonly int Area => this.area;

        public readonly int ID => this.id;

        public readonly float MaxY => this.maxY;

        public readonly bool GetEdgeAB => this.abEdge;

        public readonly bool GetEdgeAC => this.acEdge;

        public readonly bool GetEdgeBC => this.bcEdge;

        public readonly Vector2 GetAB => this.ab;
        public readonly Vector2 GetBC => this.bc;
        public readonly Vector2 GetAC => this.ac;

        #endregion

        #region Setters

#if UNITY_EDITOR
        public void SetNeighborIDs(int[] set)
        {
            this.neighborIDs.Clear();
            for (int i = 0; i < set.Length; i++)
                if (!this.neighborIDs.Contains(set[i]))
                    this.neighborIDs.Add(set[i]);
        }

        public void SetNavPointIDs(int[] set)
        {
            this.navPointIDs.Clear();
            for (int i = 0; i < set.Length; i++)
                if (!this.navPointIDs.Contains(set[i]))
                    this.navPointIDs.Add(set[i]);
        }
#endif

        public void SetIsEdge(bool abEdge, bool bcEdge, bool acEdge)
        {
            this.acEdge = acEdge;
            this.abEdge = abEdge;
            this.bcEdge = bcEdge;
        }

        #endregion

        #region In

#if UNITY_EDITOR

        public void SetBorderWidth(List<Vector3> verts, List<NavTriangle> triangles)
        {
            if (this.neighborIDs.Count == 0)
                return;

            for (int i = 0; i < this.neighborIDs.Count; i++)
                this.widthDistanceBetweenNeighbor.Add(0f);

            for (int i = 0; i < this.neighborIDs.Count; i++)
            {
                int otherID = this.neighborIDs[i];
                NavTriangle other = triangles[otherID];
                int[] ids = other.Vertices.SharedBetween(this.Vertices);

                if (ids.Length != 2)
                    continue;

                float dist = Vector3.Distance(verts[ids[0]], verts[ids[1]]);

                if (i + 2 < this.neighborIDs.Count)
                {
                    int connectedBorderNeighbor = -1;
                    if (other.neighborIDs.Contains(this.neighborIDs[i + 2]))
                        connectedBorderNeighbor = i + 2;
                    else if (other.neighborIDs.Contains(this.neighborIDs[i + 1]))
                        connectedBorderNeighbor = i + 1;

                    if (connectedBorderNeighbor > -1)
                    {
                        ids = triangles[this.neighborIDs[connectedBorderNeighbor]].Vertices
                            .SharedBetween(this.Vertices);
                        if (ids.Length == 2)
                        {
                            dist += Vector3.Distance(verts[ids[0]], verts[ids[1]]);
                            this.widthDistanceBetweenNeighbor[connectedBorderNeighbor] = dist;
                        }
                    }
                }
                else if (i + 1 < this.neighborIDs.Count)
                {
                    if (other.neighborIDs.Contains(this.neighborIDs[i + 1]))
                    {
                        ids = triangles[this.neighborIDs[i + 1]].Vertices.SharedBetween(this.Vertices);
                        if (ids.Length == 2)
                        {
                            dist += Vector3.Distance(verts[ids[0]], verts[ids[1]]);
                            this.widthDistanceBetweenNeighbor[i + 1] = dist;
                        }
                    }
                }

                this.widthDistanceBetweenNeighbor[i] = dist;
            }
        }

#endif

        #endregion

        #region Out

        public Vector3 Center(Vector3[] verts)
        {
            return Vector3.Lerp(Vector3.Lerp(verts[this.a], verts[this.b], .5f), verts[this.c], .5f);
        }

        public Vector3 Center(NavigationMesh navigationMesh)
        {
            return Vector3.Lerp(
                Vector3.Lerp(navigationMesh.VertByIndex(this.a), navigationMesh.VertByIndex(this.b), .5f),
                navigationMesh.VertByIndex(this.c), .5f);
        }

        #endregion
    }

    [Serializable]
    internal class TrianglesByVertexElement
    {
        [SerializeField] private List<int> triangleIndexes;

        public void Add(int toAdd)
        {
            this.triangleIndexes ??= new List<int>();

            this.triangleIndexes.Add(toAdd);
        }

        public int Count => this.triangleIndexes.Count;

        public int Get(int i)
        {
            return this.triangleIndexes[i];
        }

        public List<int> GetList()
        {
            return this.triangleIndexes;
        }
    }
}