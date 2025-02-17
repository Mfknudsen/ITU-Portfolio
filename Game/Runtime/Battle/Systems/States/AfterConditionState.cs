#region Packages

using System.Collections;
using System.Linq;
using Runtime.Battle.Systems.Spots;
using Runtime.Communication;
using Runtime.Player;
using Runtime.Pokémon.Conditions;
using Runtime.Systems;
using Runtime.Systems.UI;

#endregion

namespace Runtime.Battle.Systems.States
{
    public class AfterConditionState : State
    {
        private readonly SpotOversight oversight;

        public AfterConditionState(BattleSystem battleSystem, OperationManager operationManager,
            ChatManager chatManager, UIManager uiManager, PlayerManager playerManager) : base(battleSystem,
            operationManager, chatManager, uiManager, playerManager)
        {
            this.oversight = battleSystem.GetSpotOversight();
        }

        public override IEnumerator Tick()
        {
            Testing.Logger.AddLog(this.battleSystem.ToString(),"After Condition State Start");
            
            foreach (ConditionOversight conditionOversight in this.oversight.GetSpots()
                         .Where(s => s.GetActivePokemon() != null)
                         .Select(s =>
                             s.GetActivePokemon().GetConditionOversight()))
            {
                this.battleSystem.StartCoroutine(conditionOversight.CheckConditionEndTurn());

                while (!conditionOversight.GetDone() || !this.chatManager.GetIsClear())
                    yield return null;

                conditionOversight.Reset();
            }

            if (this.oversight.GetSpots().FirstOrDefault(spot =>
                    spot.GetActivePokemon() == null &&
                    spot.GetBattleMember().GetTeam().CanSendMorePokemon()))
            {
                this.battleSystem.SetState(new PlayerSelectNewState(this.battleSystem, this.operationManager, this.chatManager, this.uiManager, this.playerManager));
                yield break;
            }

            this.battleSystem.SetState(new RoundDoneState(this.battleSystem, this.operationManager, this.chatManager, this.uiManager, this.playerManager));
        }
    }
}