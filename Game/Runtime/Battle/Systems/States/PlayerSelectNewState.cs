#region Packages

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Runtime.Battle.Actions;
using Runtime.Battle.Systems.Spots;
using Runtime.Communication;
using Runtime.Player;
using Runtime.Systems;
using Runtime.Systems.UI;
using Runtime.UI.Battle.Selection;
using UnityEngine;

#endregion

namespace Runtime.Battle.Systems.States
{
    public class PlayerSelectNewState : State
    {
        public PlayerSelectNewState(BattleSystem battleSystem, OperationManager operationManager,
            ChatManager chatManager, UIManager uiManager, PlayerManager playerManager) : base(battleSystem,
            operationManager, chatManager, uiManager, playerManager)
        {
        }

        public override IEnumerator Tick()
        {
            Cursor.visible = true;
            List<SwitchAction> switchActions = new List<SwitchAction>();
            SpotOversight oversight = this.battleSystem.GetSpotOversight();
            BattleMember playerTeam = this.playerManager.GetBattleMember();

            if (playerTeam.GetTeam().CanSendMorePokemon())
            {
                foreach (Spot spot in oversight.GetSpots()
                             .Where(s =>
                                 s.GetBattleMember() == playerTeam ||
                                 s.GetActivePokemon() != null))
                {
                    SwitchAction switchAction = this.battleSystem.InstantiateSwitchAction();

                    switchAction.SetSpot(spot);

                    this.battleSystem.GetSelectionMenu().DisplaySelection(SelectorGoal.Switch, switchAction);

                    yield return new WaitWhile(() => !switchAction.GetNextPokemon() ||
                                                     !this.chatManager.GetIsClear());

                    switchActions.Add(switchAction);
                }
            }

            this.battleSystem.GetSelectionMenu().DisableDisplaySelection();

            Cursor.visible = false;

            this.battleSystem.SetState(new ComputerSelectNewState(this.battleSystem, this.operationManager, this.chatManager, this.uiManager, this.playerManager, switchActions));
        }
    }
}