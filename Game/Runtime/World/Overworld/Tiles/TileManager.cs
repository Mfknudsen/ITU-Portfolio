#region Libraries

using Runtime.Systems;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Runtime.Core;
using UnityEngine;

#endregion

namespace Runtime.World.Overworld.Tiles
{
    [CreateAssetMenu(menuName = "Manager/Tile")]
    public class TileManager : Manager
    {
        #region Values

        [SerializeField, Required] private NavMeshManager navMeshManager;
        [SerializeField, Required] private WorldManager worldManager;

        [FoldoutGroup("Tile")]
        [SerializeField]
        private float waitTime = 5;

        private TileSubController currentController;
        private readonly List<TileSubController> allSubControllers = new List<TileSubController>();

        private Timer resetWorldPositionTimer;

        private string toLoadName;

        #endregion

        #region Getters

        public TileSubController GetCurrentSubController() =>
            this.currentController;

        public TileSubController[] GetSubControllers() =>
            this.allSubControllers.ToArray();

        #endregion

        #region In

        public void AddSubManager(TileSubController add)
        {
            if (add == null || this.allSubControllers.Contains(add))
                return;

            this.allSubControllers.Add(add);

            if (this.allSubControllers.Count != 1) return;

            this.currentController = add;
            this.UpdateNavmesh();
        }

        public void RemoveSubManager(TileSubController remove)
        {
            if (remove == null || !this.allSubControllers.Contains(remove))
                return;

            this.allSubControllers.Remove(remove);
        }

        private void UpdateNavmesh()
        {
            this.currentController.StartCoroutine(this.BeginUpdate());
        }

        public void HideTiles()
        {
            TileSubController[] toDisable = this.allSubControllers.ToArray();
            foreach (TileSubController tileSubController in toDisable)
                tileSubController.gameObject.SetActive(false);
        }

        public void ShowTiles()
        {
            foreach (TileSubController tileSubController in this.allSubControllers)
                tileSubController.gameObject.SetActive(true);
        }

        public void SetCurrentSubTile(string newSubManagerName)
        {
            if (this.toLoadName.Equals(newSubManagerName)) return;

            this.toLoadName = newSubManagerName;

            this.resetWorldPositionTimer?.Stop();

            this.resetWorldPositionTimer = new Timer(this.waitTime,
                () =>
                {
                    this.currentController.DisableDividers();

                    List<Neighbor> toUnload = new List<Neighbor>(),
                        loaded = new List<Neighbor>(),
                        toLoad = new List<Neighbor>();
                    toUnload.AddRange(this.currentController.GetNeighbors());
                    loaded.AddRange(this.currentController.GetNeighbors());

                    this.currentController = this.allSubControllers.First(m => m.GetTileName().Equals(newSubManagerName));
                    this.currentController.EnableDividers();

                    foreach (Neighbor neighbor in this.currentController.GetNeighbors().Where(n => toUnload.Contains(n)))
                        toUnload.Remove(neighbor);

                    foreach (Neighbor neighbor in toUnload.Where(n => loaded.Contains(n)))
                        loaded.Remove(neighbor);

                    toLoad.AddRange(this.currentController.GetNeighbors().Where(n => loaded.Contains(n)));

                    #region Unload Unneeded Neighbors

                    foreach (TileSubController unload in toUnload.Select(neighbor =>
                                 this.allSubControllers.First(m => m.GetTileName().Equals(neighbor.GetSceneName()))))
                        unload.Unload();

                    #endregion

                    #region Load New Neighbors

                    foreach (Neighbor neighbor in toLoad)
                    {
                        this.worldManager.LoadSceneAsync(neighbor.GetSceneName());
                    }

                    #endregion

                    this.ResetWorldCenter();
                    this.UpdateNavmesh();
                });
        }

        #endregion

        #region Out

        public TileSubController GetSubManagerByName(string tileName) =>
            this.allSubControllers.FirstOrDefault(tileSubManager => tileSubManager.GetTileName().Equals(tileName));

        #endregion

        #region Internal

        private IEnumerator BeginUpdate()
        {
            yield return this.navMeshManager.RebakeWait(this.currentController.GetSurface());
        }

        private void ResetWorldCenter()
        {
        }

        #endregion
    }
}