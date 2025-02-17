#region Packages

using System.Collections;
using Runtime.Communication;
using Runtime.Player;
using Runtime.Systems;
using Runtime.Systems.UI;

#endregion

namespace Runtime.Battle.Systems.States
{
    public abstract class State
    {
        protected readonly BattleSystem battleSystem;
        protected readonly OperationManager operationManager;
        protected readonly ChatManager chatManager;
        protected readonly UIManager uiManager;
        protected readonly PlayerManager playerManager;

        protected State(BattleSystem battleSystem, OperationManager operationManager, ChatManager chatManager, UIManager uiManager, PlayerManager playerManager)
        {
            this.battleSystem = battleSystem;
            this.operationManager = operationManager;
            this.chatManager = chatManager;
            this.uiManager = uiManager;
            this.playerManager = playerManager;
        }

        public abstract IEnumerator Tick();
    }
}