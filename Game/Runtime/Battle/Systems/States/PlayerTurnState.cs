#region Packages

using System.Collections;
using System.Linq;
using Runtime.Battle.Systems.Spots;
using Runtime.Communication;
using Runtime.Player;
using Runtime.Pokémon;
using Runtime.Systems;
using Runtime.Systems.UI;
using Runtime.UI.Battle.Selection;
using UnityEngine;

#endregion

namespace Runtime.Battle.Systems.States
{
    public class PlayerTurnState : State
    {
        public PlayerTurnState(BattleSystem battleSystem, OperationManager operationManager, ChatManager chatManager,
            UIManager uiManager, PlayerManager playerManager) : base(battleSystem, operationManager, chatManager,
            uiManager, playerManager)
        {
        }

        public override IEnumerator Tick()
        {
            Testing.Logger.AddLog(this.battleSystem.ToString(), "Player Turn State Start");

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            SpotOversight spotOversight = this.battleSystem.GetSpotOversight();

            this.chatManager.ShowTextField(false);

            foreach (Pokemon pokemon in spotOversight.GetSpots()
                         .Where(spot => spot.GetBattleMember().IsPlayer() && spot.GetActivePokemon() != null)
                         .Select(spot => spot.GetActivePokemon())
                         .ToArray())
            {
                this.battleSystem.GetSelectionMenu().DisplaySelection(SelectorGoal.Turn, pokemon);

                while (pokemon.GetBattleAction() == null)
                    yield return null;
            }

            this.battleSystem.GetSelectionMenu().DisableDisplaySelection();

            this.battleSystem.SetState(new ComputerTurnState(this.battleSystem, this.operationManager, this.chatManager,
                this.uiManager, this.playerManager));
        }
    }
}