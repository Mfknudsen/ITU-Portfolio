#region Libraries

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Runtime.AI.Navigation;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

#endregion

namespace Editor.Tests
{
    public sealed class AvoidanceTestLarge : MonoBehaviour
    {
        private IEnumerator Start()
        {
            UnitNavigation.SetNavMesh(this.navMesh);

            yield return new WaitWhile(() => !UnitNavigation.Ready);

            while (this.team1.Any(agent => !agent.HasEntity() && agent.gameObject.activeSelf) ||
                   this.team2.Any(agent => !agent.HasEntity() && agent.gameObject.activeSelf))
                yield return null;

            for (int i = 0; i < this.team1.Length; i++)
            {
                if (this.team1[i].gameObject.activeSelf)
                    this.team1[i].MoveTo(this.team2[i].transform.position);

                if (this.team2[i].gameObject.activeSelf)
                    this.team2[i].MoveTo(this.team1[i].transform.position);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            return;
            List<Vector2Int> drawn = new List<Vector2Int>();

            foreach (NavTriangle t in this.navMesh.Triangles)
            {
                if (!drawn.Contains(new Vector2Int(t.GetA, t.GetB)) && !drawn.Contains(new Vector2Int(t.GetB, t.GetA)))
                {
                    drawn.Add(new Vector2Int(t.GetA, t.GetB));
                    Debug.DrawLine(this.navMesh.VertByIndex(t.GetA), this.navMesh.VertByIndex(t.GetB));
                }

                if (!drawn.Contains(new Vector2Int(t.GetC, t.GetB)) && !drawn.Contains(new Vector2Int(t.GetB, t.GetC)))
                {
                    drawn.Add(new Vector2Int(t.GetC, t.GetB));
                    Debug.DrawLine(this.navMesh.VertByIndex(t.GetC), this.navMesh.VertByIndex(t.GetB));
                }

                if (!drawn.Contains(new Vector2Int(t.GetA, t.GetC)) && !drawn.Contains(new Vector2Int(t.GetC, t.GetA)))
                {
                    drawn.Add(new Vector2Int(t.GetA, t.GetC));
                    Debug.DrawLine(this.navMesh.VertByIndex(t.GetA), this.navMesh.VertByIndex(t.GetC));
                }

                Handles.Label(t.Center(this.navMesh), t.ID.ToString());
            }
        }
#endif

        #region Values

        [SerializeField] private NavigationAgent[] team1, team2;

        [SerializeField] [Required] private NavigationMesh navMesh;

        #endregion
    }
}