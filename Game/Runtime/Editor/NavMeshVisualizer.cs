#region Libraries

using System.Linq;
using Runtime.AI.Navigation;
using Runtime.World.Overworld;
using UnityEditor;
using UnityEngine;

#endregion

namespace Runtime.Editor
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public sealed class NavMeshVisualizer : MonoBehaviour
    {
        #region Build In States

#if UNITY_EDITOR
        public void Create()
        {
            Mesh mesh = new Mesh { name = "Navmesh" };

            NavigationMesh calculatedNavmesh = this.transform.root.GetComponent<TileSubController>().GetNavmesh;

            mesh.SetVertices(calculatedNavmesh.Vertices());
            mesh.SetIndices(calculatedNavmesh.Triangles.SelectMany(t => t.Vertices).ToArray(), MeshTopology.Triangles,
                0);
            mesh.SetNormals(calculatedNavmesh.Vertices().Select(v => Vector3.up).ToArray());
            mesh.RecalculateNormals();

            this.GetComponent<MeshRenderer>().material =
                AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Editor/NavmeshVisualizer.mat");
            this.GetComponent<MeshFilter>().mesh = mesh;
        }
#endif

        #endregion
    }
}