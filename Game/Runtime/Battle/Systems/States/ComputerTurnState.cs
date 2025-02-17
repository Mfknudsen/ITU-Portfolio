#region Packages

using System.Collections;
using System.Linq;
using Runtime.Battle.Systems.Spots;
using Runtime.Communication;
using Runtime.Player;
using Runtime.Pokémon;
using Runtime.Systems;
using Runtime.Systems.UI;

#endregion

namespace Runtime.Battle.Systems.States
{
    public sealed class ComputerTurnState : State
    {
        public ComputerTurnState(BattleSystem battleSystem, OperationManager operationManager, ChatManager chatManager,
            UIManager uiManager, PlayerManager playerManager) : base(battleSystem, operationManager, chatManager,
            uiManager, playerManager)
        {
        }

        public override IEnumerator Tick()
        {
            Testing.Logger.AddLog(this.battleSystem.ToString(), "Computer Turn State Start");

            SpotOversight spotOversight = this.battleSystem.GetSpotOversight();

            foreach (Spot spot in spotOversight.GetSpots().Where(spot =>
                         spot.GetActivePokemon() != null &&
                         !spot.GetBattleMember().IsPlayer()))
            {
                BattleMember battleMember = spot.GetBattleMember();
                Pokemon pokemon = spot.GetActivePokemon();

                battleMember.ActivateAIBrain(pokemon);

                while (pokemon.GetBattleAction() == null)
                    yield return null;
            }

            this.battleSystem.SetState(new ActionState(this.battleSystem, this.operationManager, this.chatManager,
                this.uiManager, this.playerManager));
        }
    }
}