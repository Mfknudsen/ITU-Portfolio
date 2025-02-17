#region Libraries

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Runtime.AI.Navigation;
using Runtime.World.Overworld.TileHierarchy;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Editor.Tests
{
    public sealed class AvoidanceTestSmall : MonoBehaviour
    {
        #region Values

        [SerializeField] private GameObject agentPrefab;

        [SerializeField] private NavigationMesh navMesh;

        [SerializeField] private List<UnitAgent> agents;

        private const int SpawnCount = 16;
        private const float Rotate = 360f / SpawnCount, SpawnDistance = 7f;

        #endregion

        #region Build In States

        private void OnDrawGizmos()
        {
            Vector3[] verts = this.navMesh.Vertices();

            foreach (NavTriangle navMeshTriangle in this.navMesh.Triangles)
            {
                if (navMeshTriangle.GetEdgeAB)
                    Debug.DrawLine(verts[navMeshTriangle.GetA] + Vector3.up * 1.5f,
                        verts[navMeshTriangle.GetB] + Vector3.up * 1.5f, Color.red);
                if (navMeshTriangle.GetEdgeAC)
                    Debug.DrawLine(verts[navMeshTriangle.GetA] + Vector3.up * 1.5f,
                        verts[navMeshTriangle.GetC] + Vector3.up * 1.5f, Color.red);
                if (navMeshTriangle.GetEdgeBC)
                    Debug.DrawLine(verts[navMeshTriangle.GetC] + Vector3.up * 1.5f,
                        verts[navMeshTriangle.GetB] + Vector3.up * 1.5f, Color.red);
            }
        }

        private IEnumerator Start()
        {
            UnitNavigation.SetNavMesh(this.navMesh);

            while (this.agents.Any(agent => !agent.IsOnNavMesh() && agent.gameObject.activeSelf))
                yield return null;

            for (int i = 0; i < this.agents.Count; i++)
            {
                int other = i + this.agents.Count / 2;
                if (other >= this.agents.Count)
                    other -= this.agents.Count;

                this.agents[i].MoveTo(this.agents[other].transform.position);
            }

            while (this.agents.Any(agent => !agent.HasPath()))
                yield return null;

            Debug.Log("All have path");

            foreach (UnitAgent unitAgent in this.agents)
                unitAgent.SetStopped(false);
        }

        #endregion

        #region Internal

        [Button]
        private void SpawnAgents()
        {
            if (this.agentPrefab == null)
                return;

            if (this.agentPrefab.GetComponent<UnitAgent>() == null)
                return;

            if (this.agents != null)
            {
                while (this.agents.Count > 0)
                {
                    UnitAgent a = this.agents[0];
                    this.agents.RemoveAt(0);
                    DestroyImmediate(a.gameObject);
                }
            }
            else
                this.agents = new List<UnitAgent>();

            Quaternion startRotation = this.transform.rotation;
            Transform parent = FindObjectOfType<TileAI>().transform;

            for (int i = 0; i < SpawnCount; i++)
            {
                this.transform.rotation = startRotation;
                this.transform.Rotate(Vector3.up, Rotate * i);
                this.agents.Add(Instantiate(this.agentPrefab,
                    this.transform.position + this.transform.forward * SpawnDistance,
                    Quaternion.LookRotation(-this.transform.forward), parent).GetComponent<UnitAgent>());
            }
        }

        #endregion
    }
}