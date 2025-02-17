#region Packages

using System.Collections;
using Runtime.Battle.Systems.Interfaces;
using Runtime.Battle.Systems.Spots;
using Runtime.Communication;
using Runtime.Player;
using Runtime.Systems;
using Runtime.Systems.UI;
using UnityEngine;

#endregion

// ReSharper disable SuspiciousTypeConversion.Global
namespace Runtime.Battle.Systems.States
{
    public class RoundDoneState : State
    {
        public RoundDoneState(BattleSystem battleSystem, OperationManager operationManager, ChatManager chatManager,
            UIManager uiManager, PlayerManager playerManager) : base(battleSystem, operationManager, chatManager,
            uiManager, playerManager)
        {
        }

        public override IEnumerator Tick()
        {
            Testing.Logger.AddLog(this.battleSystem.ToString(), "Round Done State Start");

            #region End Turn Abilities

            AbilityOversight abilityOversight = this.battleSystem.GetAbilityOversight();
            foreach (IOnTurnEnd onTurnEnd in abilityOversight.ListOfSpecific<IOnTurnEnd>())
            {
                if (onTurnEnd is not IOperation iOperation) continue;

                OperationsContainer container = new OperationsContainer();
                container.Add(iOperation);
                this.operationManager.AddOperationsContainer(container);

                yield return new WaitWhile(() => !this.operationManager.GetDone());
            }

            #endregion

            #region End Battle

            if (this.battleSystem.CheckTeamDefeated(true))
                this.battleSystem.SetState(new LostState(this.battleSystem, this.operationManager, this.chatManager,
                    this.uiManager, this.playerManager));
            else if (this.battleSystem.CheckTeamDefeated(false))
                this.battleSystem.SetState(new WinState(this.battleSystem, this.operationManager, this.chatManager,
                    this.uiManager, this.playerManager));
            else
            {
                SpotOversight spotOversight = this.battleSystem.GetSpotOversight();

                spotOversight.Reorganise(true);

                this.battleSystem.SetState(new PlayerTurnState(this.battleSystem, this.operationManager,
                    this.chatManager, this.uiManager, this.playerManager));
            }

            #endregion
        }
    }
}