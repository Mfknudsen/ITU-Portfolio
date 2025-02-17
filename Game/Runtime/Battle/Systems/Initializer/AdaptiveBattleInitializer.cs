#region Packages

using System.Collections;
using UnityEngine;

#endregion

namespace Runtime.Battle.Systems.Initializer
{
    public sealed class AdaptiveBattleInitializer : BattleInitializer
    {
        #region Values

        [SerializeField, Min(4)] private int scanZones;

        [SerializeField, Min(1)] private int checksPerFrame;


        private Transform playerCharacterPosition;

        private Transform[] allySpots, enemySpots, allyCharacterCharacterPositions, enemyCharacterCharacterPositions;

        #endregion

        #region Getters

        public override Transform[] GetAllySpots() => this.allySpots;

        public override Transform[] GetEnemySpots() => this.enemySpots;

        public override Transform GetPlayerCharacterPosition() => this.playerCharacterPosition;

        public override Transform[] GetAllyCharacterPositions() => this.allyCharacterCharacterPositions;

        public override Transform[] GetEnemyCharacterPositions() => this.enemyCharacterCharacterPositions;

        #endregion

        #region In

        public override void FindSetupBattleZone()
        {
            base.FindSetupBattleZone();

            this.StartCoroutine(this.FindZone());
        }

        #endregion


        #region Internal

        private IEnumerator FindZone()
        {
            int checkCount = 0;
            while (!this.found)
            {
                int allowed = 0;
                for (int i = 0; i < this.scanZones; i++)
                {
                    if (!this.IsZoneClearOfObstacles())
                        break;

                    if (this.IsZoneReachable())
                        break;

                    allowed++;
                }

                if (allowed == checkCount)
                {
                    this.found = true;
                    break;
                }

                checkCount++;

                if (checkCount < this.checksPerFrame)
                    continue;

                checkCount = 0;
                yield return null;
            }
        }

        private bool IsZoneReachable()
        {
            return true;
        }

        private bool IsZoneClearOfObstacles()
        {
            return true;
        }

        #endregion
    }
}