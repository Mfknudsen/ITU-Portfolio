#region SDK

using System.Collections;
using Runtime.Communication;
using Runtime.Player;
using Runtime.Systems;
using Runtime.Systems.UI;

#endregion

namespace Runtime.Battle.Systems.States
{
    public class LostState : State
    {
        public LostState(BattleSystem battleSystem, OperationManager operationManager, ChatManager chatManager,
            UIManager uiManager, PlayerManager playerManager) : base(battleSystem, operationManager, chatManager,
            uiManager, playerManager)
        {
        }

        public override IEnumerator Tick()
        {
            this.battleSystem.EndBattle(false);

            yield break;
        }
    }
}