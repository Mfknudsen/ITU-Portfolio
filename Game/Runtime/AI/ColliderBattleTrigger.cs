#region Packages

using Runtime.Battle.Systems;
using Runtime.Battle.Systems.BattleStart;
using UnityEngine;

#endregion

namespace Runtime.AI
{
    public class ColliderBattleTrigger : MonoBehaviour
    {
        #region Values

        [SerializeField] private BattleStarter battleStarter;

        #endregion

        #region Build In States

        private void OnTriggerEnter(Collider other)
        {
            if (!this.battleStarter.GetIsBattleReady()) 
                return;

            this.battleStarter.TriggerBattle();
        }

        #endregion
    }
}