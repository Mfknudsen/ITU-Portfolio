#region Libraries

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Runtime.World.Overworld
{
    [Serializable]
    public struct TileOptimizedInformation
    {
        #region Values

        private readonly int groupingSize;

        private readonly float lowestX,
            lowestY,
            highestX,
            highestY;

        [SerializeField] private List<OptimizedTreeInstance>[,] groupedTerrainTrees;

        #endregion

        #region Build In States

        public TileOptimizedInformation(int groupingSize, float lowestX, float lowestY, float highestX, float highestY,
            List<OptimizedTreeInstance>[,] groupedTerrainTrees)
        {
            this.groupingSize = groupingSize;
            this.lowestX = lowestX;
            this.lowestY = lowestY;
            this.highestX = highestX;
            this.highestY = highestY;

            this.groupedTerrainTrees = groupedTerrainTrees;
        }

        #endregion

        #region Getters

        public IReadOnlyList<OptimizedTreeInstance>[,] GetTreeInstances => this.groupedTerrainTrees;

        public readonly float GetLowestX() => this.lowestX;

        public readonly float GetLowestY() => this.lowestY;

        public readonly float GetHighestX() => this.highestX;

        public readonly float GetHighestY() => this.highestY;

        public readonly int GetGroupingSize() => this.groupingSize;

        #endregion
    }

    public readonly struct OptimizedTreeInstance
    {
        #region Values

        public readonly Vector3 position;

        public readonly float height, radius;

        #endregion

        #region Build In States

        public OptimizedTreeInstance(TreeInstance instance)
        {
            this.position = instance.position;
            this.height = 5;
            this.radius = 2;
        }

        #endregion
    }
}