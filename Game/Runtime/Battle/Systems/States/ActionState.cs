#region Packages

using System.Collections;
using System.Linq;
using Runtime.Battle.Actions;
using Runtime.Battle.Systems.Spots;
using Runtime.Communication;
using Runtime.Player;
using Runtime.Pokémon;
using Runtime.Pokémon.Conditions.Non_Volatiles;
using Runtime.Systems;
using Runtime.Systems.UI;
using UnityEngine;
using Logger = Runtime.Testing.Logger;

#endregion

namespace Runtime.Battle.Systems.States
{
    public class ActionState : State
    {
        private readonly SpotOversight spotOversight;

        public ActionState(BattleSystem battleSystem, OperationManager operationManager, ChatManager chatManager,
            UIManager uiManager, PlayerManager playerManager) : base(battleSystem, operationManager, chatManager,
            uiManager, playerManager)
        {
            this.spotOversight = battleSystem.GetSpotOversight();
        }

        public override IEnumerator Tick()
        {
            Logger.AddLog(this.battleSystem, "Action State Start");

            Pokemon[] withActions = this.spotOversight.GetSpots()
                .Select(s => s.GetActivePokemon())
                .OrderBy(p => p.GetStatRaw(Stat.Speed))
                .ToArray();

            while (withActions.Length > 0)
            {
                Pokemon pokemon = withActions[0];

                Logger.AddLog(this.battleSystem, "Starting action for: " + pokemon.GetName());

                #region Start Action

                BattleAction action = pokemon.GetBattleAction();

                Logger.instance.AddNewLog(action.name, "Starting Action: " + action.name.Replace("(Clone)", ""));

                OperationsContainer container = new OperationsContainer(action);
                this.operationManager.AddOperationsContainer(container);

                yield return new WaitWhile(() => !this.operationManager.GetDone() || !this.chatManager.GetIsClear());

                #endregion

                #region Check Any Fainted

                foreach (Pokemon checkPokemon in this.spotOversight.GetSpots()
                             .Select(s =>
                                 s.GetActivePokemon())
                             .Where(p =>
                                 p.GetCurrentHealth() == 0))
                {
                    this.battleSystem.SetPokemonFainted(checkPokemon);

                    if (checkPokemon.GetConditionOversight()
                            .GetNonVolatileStatus() is FaintedCondition faintedCondition)
                    {
                        this.operationManager.AddOperationsContainer(new OperationsContainer(faintedCondition));

                        while (!faintedCondition.IsOperationDone)
                            yield return null;
                    }

                    while (!this.operationManager.GetDone())
                        yield return null;
                }

                #endregion

                #region Recheck Order

                withActions = this.spotOversight.GetSpots()
                    .Select(s => s.GetActivePokemon())
                    .Where(p =>
                        p != null &&
                        p.GetConditionOversight().GetNonVolatileStatus() is not FaintedCondition &&
                        p.GetBattleAction() != null)
                    .OrderBy(p => p.GetStatRaw(Stat.Speed))
                    .ToArray();

                #endregion
            }

            if (this.battleSystem.CheckTeamDefeated(true) ||
                this.battleSystem.CheckTeamDefeated(false))
            {
                this.battleSystem.SetState(new RoundDoneState(this.battleSystem, this.operationManager,
                    this.chatManager, this.uiManager, this.playerManager));

                yield break;
            }

            this.battleSystem.SetState(new AfterConditionState(this.battleSystem, this.operationManager,
                this.chatManager, this.uiManager, this.playerManager));
        }
    }
}