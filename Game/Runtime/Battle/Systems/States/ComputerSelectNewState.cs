#region Packages

using System.Collections;
using System.Collections.Generic;
using Runtime.Battle.Actions;
using Runtime.Battle.Systems.Spots;
using Runtime.Communication;
using Runtime.Player;
using Runtime.Systems;
using Runtime.Systems.UI;

#endregion

namespace Runtime.Battle.Systems.States
{
    public class ComputerSelectNewState : State
    {
        private readonly List<SwitchAction> switchActions;

        public ComputerSelectNewState(BattleSystem battleSystem, OperationManager operationManager,
            ChatManager chatManager, UIManager uiManager, PlayerManager playerManager, List<SwitchAction> switchActions)
            : base(battleSystem,
                operationManager, chatManager, uiManager, playerManager)
        {
            this.switchActions = switchActions;
        }

        public override IEnumerator Tick()
        {
            BattleMember playerBattleMember = this.playerManager.GetBattleMember();

            foreach (Spot spot in this.battleSystem.GetSpotOversight().GetSpots())
            {
                BattleMember battleMember = spot.GetBattleMember();

                if (spot.GetActivePokemon() == null ||
                    battleMember == playerBattleMember ||
                    !battleMember.GetTeam().CanSendMorePokemon()) continue;

                battleMember.ActivateAIBrain(spot.GetActivePokemon());
            }

            this.battleSystem.SetState(new SwitchNewInState(this.battleSystem, this.operationManager, this.chatManager, this.uiManager, this.playerManager, this.switchActions));

            yield break;
        }
    }
}