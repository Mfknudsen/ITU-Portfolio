#region Packages

using UnityEngine;

#endregion

namespace Runtime.Battle.Systems.Initializer
{
    public abstract class BattleInitializer : MonoBehaviour
    {
        #region Values

        protected bool found;

        #endregion

        #region Getters

        public bool HasFoundBattleZone => this.found;

        public abstract Transform[] GetAllySpots();

        public abstract Transform[] GetEnemySpots();

        public abstract Transform GetPlayerCharacterPosition();

        public abstract Transform[] GetAllyCharacterPositions();

        public abstract Transform[] GetEnemyCharacterPositions();

        #endregion

        #region In

        public virtual void FindSetupBattleZone() =>
            this.found = false;

        #endregion
    }
}