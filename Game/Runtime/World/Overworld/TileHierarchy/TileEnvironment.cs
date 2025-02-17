#region Libraries

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Runtime.World.Overworld.TileHierarchy
{
    public sealed class TileEnvironment : TileHierarchy
    {
        #region Values

        [SerializeField] private List<Terrain> terrains = new List<Terrain>();

        #endregion

        #region Getters

        public List<Terrain> GetTerrains() => this.terrains;

        #endregion
    }
}