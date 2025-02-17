#region Packages

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Runtime.Battle.Actions;
using Runtime.Battle.Systems.BattleStart;
using Runtime.Battle.Systems.Initializer;
using Runtime.Battle.Systems.Spots;
using Runtime.Communication;
using Runtime.Player;
using Runtime.Pokémon;
using Runtime.Systems;
using Runtime.Systems.UI;
using UnityEngine;
using Logger = Runtime.Testing.Logger;

#endregion

namespace Runtime.Battle.Systems.States
{
    public class BeginState : State
    {
        #region Values

        private SpotOversight spotOversight;
        private readonly BattleInitializer battleInitializer;

        #endregion

        #region Build In States

        public BeginState(BattleSystem battleSystem, OperationManager operationManager, ChatManager chatManager,
            UIManager uiManager, PlayerManager playerManager, BattleInitializer battleInitializer) : base(battleSystem,
            operationManager, chatManager,
            uiManager, playerManager)
        {
            this.battleInitializer = battleInitializer;
        }

        #endregion

        #region In

        public override IEnumerator Tick()
        {
            this.SetupSpots();

            this.SetupUI();

            this.SetupAbilityOversight();

            yield return this.StartLog();

            yield return this.SetupActions();

            this.spotOversight.Reorganise(false);

            this.battleSystem.SetState(new PlayerTurnState(this.battleSystem, this.operationManager, this.chatManager,
                this.uiManager, this.playerManager));
        }

        #endregion

        #region Internal

        private void SetupSpots()
        {
            BattleStarter battleStarter = this.battleSystem.GetStarter();

            foreach (BattleMember battleMember in battleStarter.GetAllBattleMembers())
            {
                battleMember.Setup();
                battleMember.GetTeam().Setup();
            }

            this.spotOversight = this.battleSystem.GetSpotOversight();

            BattleMember[] playerWithAllies = { this.playerManager.GetBattleMember() };
            this.spotOversight.SetupSpots(playerWithAllies.Concat(battleStarter.GetAllies()).ToArray(),
                this.battleInitializer.GetAllySpots(),
                this.battleSystem);
            this.spotOversight.SetupSpots(battleStarter.GetEnemies(),
                this.battleInitializer.GetEnemySpots(),
                this.battleSystem);

            this.spotOversight.Reorganise(false);
        }

        private IEnumerator StartLog()
        {
            string playersMsg = "Starting Battle Between:";
            string alliesMsg = " - " + this.playerManager.GetBattleMember().GetName(), enemiesMsg = "";

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (Spot spot in this.battleSystem.GetSpotOversight().GetSpots())
            {
                BattleMember battleMember = spot.GetBattleMember();

                if (!battleMember || battleMember == this.playerManager.GetBattleMember()) continue;

                if (battleMember.GetTeamAffiliation() == this.playerManager.GetBattleMember().GetTeamAffiliation())
                    alliesMsg += ", " + battleMember.GetName();
                else
                {
                    if (!enemiesMsg.Equals(""))
                        enemiesMsg += ", ";
                    else
                        enemiesMsg += " - ";

                    enemiesMsg += battleMember.GetName();
                }
            }

            playersMsg += "\n" + alliesMsg + "\n" + enemiesMsg;

            yield return new WaitWhile(() => !Logger.instance);

            Logger.AddLog("Begin State", playersMsg);

            while (!this.chatManager.GetIsClear())
                yield return null;
        }

        private void SetupUI() =>
            this.battleSystem.GetSelectionMenu().Setup();

        private void SetupAbilityOversight() =>
            this.battleSystem.SetupAbilityOversight();

        private IEnumerator SetupActions()
        {
            List<OperationsContainer> switchInActions = new List<OperationsContainer>();
            foreach (Spot spot in this.spotOversight.GetSpots())
            {
                spot.SetTransform();

                if (spot.GetBattleMember() is null)
                    continue;

                BattleMember battleMember = spot.GetBattleMember();
                Pokemon pokemon = battleMember.GetTeam().GetFirstOut();

                if (!pokemon) continue;

                SwitchAction action = this.battleSystem.InstantiateSwitchAction();

                action.SetNextPokemon(battleMember.GetTeam().GetFirstOut());
                action.SetSpot(spot);

                OperationsContainer container = new OperationsContainer();
                container.Add(action);
                switchInActions.Add(container);
            }

            this.operationManager.AddOperationsContainer(switchInActions.ToArray());

            foreach (IOperation i in switchInActions
                         .SelectMany(switchInAction =>
                             switchInAction.GetInterfaces()))
            {
                while (!i.IsOperationDone)
                    yield return null;
            }

            while (!this.chatManager.GetIsClear() || !this.operationManager.GetDone())
                yield return null;
        }

        #endregion
    }
}