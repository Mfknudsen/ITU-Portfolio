#region Packages

using System.Collections;
using System.Linq;
using Runtime.Battle.Systems.Interfaces;
using Runtime.Communication;
using Runtime.Player;
using Runtime.Systems;
using Runtime.Systems.UI;

#endregion

namespace Runtime.Battle.Systems.States
{
    public class WeatherState : State
    {
        public WeatherState(BattleSystem battleSystem, OperationManager operationManager, ChatManager chatManager, UIManager uiManager, PlayerManager playerManager) : base(battleSystem, operationManager, chatManager, uiManager, playerManager)
        {
        }

        public override IEnumerator Tick()
        {
            foreach (IOnTurnEnd onTurnEnd in this.battleSystem.GetWeatherManager().GetWeatherWithInterface<IOnTurnEnd>()
                .Where(i => i is IOperation))
            {
                OperationsContainer container = new OperationsContainer();
                container.Add((IOperation)onTurnEnd);
                this.operationManager.AddOperationsContainer(container);
            }

            this.battleSystem.SetState(new RoundDoneState(this.battleSystem, this.operationManager, this.chatManager, this.uiManager, this.playerManager));
            yield break;
        }
    }
}