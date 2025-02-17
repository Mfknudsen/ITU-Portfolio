#region Libraries

using System.Collections.Generic;
using Runtime.AI.Navigation;
using Runtime.Algorithms.PathFinding;
using Runtime.Editor.Tests;
using UnityEngine;

#endregion

namespace Runtime
{
    public sealed class NavTester : MonoBehaviour
    {
        #region Values

        [SerializeField] private SavedUnitPath savedUnitPath;

        [SerializeField] private NavTriangle[] triangles;

        [SerializeField] private Transform[] vertices;

        private NavigationMesh temp;

        private List<Portal> portals;

        private List<Vector3> path;

        [SerializeField] private Transform start, end;

        private Dictionary<int, RemappedVert> remapped;

        private List<Vector3> rights, lefts;

        #endregion

        #region Build In States

        private void Start()
        {
            this.temp = ScriptableObject.CreateInstance<NavigationMesh>();
            List<Vector3> verts = new List<Vector3>();
            if (this.savedUnitPath == null)
            {
                foreach (Transform t in this.vertices) verts.Add(t.position);

                this.temp.SetValues(verts.ToArray(),
                    this.triangles,
                    new int[this.triangles.Length],
                    new Dictionary<int, List<NavigationPointEntry>>());

                UnitNavigation.SetNavMesh(this.temp);

                List<int> triangleIDs = new List<int>();
                for (int i = 0; i < this.triangles.Length; i++)
                    triangleIDs.Add(i);

                this.path = Funnel.GetPath(this.start.position,
                    this.end.position,
                    triangleIDs.ToArray(),
                    this.triangles,
                    verts.ToArray(),
                    this.start.GetComponent<UnitAgent>());
            }
            else
            {
                List<Vector3> list = this.savedUnitPath.GetPathPoints();
                List<NavTriangle> navTriangles = new List<NavTriangle>();
                for (int i = 0; i < list.Count; i += 3)
                {
                    Vector3 vecA = list[i];
                    if (!verts.Contains(vecA))
                        verts.Add(vecA);
                    int a = verts.IndexOf(vecA);
                    Vector3 vecB = list[i + 1];
                    if (!verts.Contains(vecB))
                        verts.Add(vecB);
                    int b = verts.IndexOf(vecB);
                    Vector3 vecC = list[i + 2];
                    if (!verts.Contains(vecC))
                        verts.Add(vecC);
                    int c = verts.IndexOf(vecC);

                    NavTriangle triangle = new NavTriangle(i / 3, a, b, c, 0, vecA, vecB, vecC);

                    List<int> neighbors = new List<int>();
                    if (i / 3 != 0)
                        neighbors.Add(i / 3 - 1);
                    if (i + 3 < list.Count)
                        neighbors.Add(i / 3 + 1);

                    triangle.SetNeighborIDs(neighbors.ToArray());
                    navTriangles.Add(triangle);
                }

                this.temp.SetValues(verts.ToArray(),
                    navTriangles.ToArray(),
                    new int[navTriangles.Count],
                    new Dictionary<int, List<NavigationPointEntry>>());

                UnitNavigation.SetNavMesh(this.temp);

                List<int> triangleIDs = new List<int>();
                for (int i = 0; i < navTriangles.Count; i++)
                    triangleIDs.Add(i);

                this.path = Funnel.GetPath(this.savedUnitPath.GetStart(),
                    this.savedUnitPath.GetEnd(),
                    triangleIDs.ToArray(),
                    navTriangles.ToArray(),
                    verts.ToArray(),
                    this.start.GetComponent<UnitAgent>());
            }
        }

        private void OnDestroy()
        {
            Destroy(this.temp);
        }

        private void OnDrawGizmos()
        {
            if (this.savedUnitPath != null)
            {
                Debug.DrawLine(this.savedUnitPath.GetStart(), this.savedUnitPath.GetStart() + Vector3.up,
                    Color.green);
                Debug.DrawLine(this.savedUnitPath.GetEnd(), this.savedUnitPath.GetEnd() + Vector3.up, Color.magenta);

                List<Vector3> list = this.savedUnitPath.GetPathPoints();
                for (int i = 0; i < list.Count; i += 3)
                {
                    Vector3 vecA = list[i];
                    Vector3 vecB = list[i + 1];
                    Vector3 vecC = list[i + 2];
                    Debug.DrawLine(vecA, vecB, Color.red);
                    Debug.DrawLine(vecC, vecB, Color.red);
                    Debug.DrawLine(vecC, vecA, Color.red);
                }
            }
            else
            {
                foreach (NavTriangle navTriangle in this.triangles)
                {
                    Vector3 a = this.vertices[navTriangle.GetA].position,
                        b = this.vertices[navTriangle.GetB].position,
                        c = this.vertices[navTriangle.GetC].position;
                    Debug.DrawLine(a, b, Color.red);
                    Debug.DrawLine(c, b, Color.red);
                    Debug.DrawLine(c, a, Color.red);
                }
            }

            if (this.path is { Count: > 0 })
            {
                Debug.DrawLine(this.savedUnitPath == null ? this.start.position : this.savedUnitPath.GetStart(),
                    this.path[0], Color.yellow);

                for (int i = 1; i < this.path.Count; i++)
                    Debug.DrawLine(this.path[i], this.path[i - 1], Color.yellow);
            }

            if (this.rights != null)
                for (int i = 1; i < this.rights.Count; i++)
                    Debug.DrawLine(this.rights[i] + Vector3.up, this.rights[i - 1] + Vector3.up, Color.green);

            if (this.lefts != null)
                for (int i = 1; i < this.lefts.Count; i++)
                    Debug.DrawLine(this.lefts[i] + Vector3.up, this.lefts[i - 1] + Vector3.up, Color.blue);
        }

        #endregion
    }
}