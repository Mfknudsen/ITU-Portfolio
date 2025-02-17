#region Packages

using System.Collections;
using System.Linq;
using Runtime.Communication;
using Runtime.Player;
using Runtime.Systems;
using Runtime.Systems.UI;

#endregion

namespace Runtime.Battle.Systems.States
{
    public class WinState : State
    {
        public WinState(BattleSystem battleSystem, OperationManager operationManager, ChatManager chatManager, UIManager uiManager, PlayerManager playerManager) : base(battleSystem, operationManager, chatManager, uiManager, playerManager)
        {
        }

        public override IEnumerator Tick()
        {
            foreach (BattleMember battleMember in this.battleSystem.GetSpotOversight().GetSpots()
                         .Select(s =>
                             s.GetBattleMember())
                         .Where(bm =>
                             !bm.GetTeamAffiliation()))
                this.chatManager.Add(battleMember.GetOnDefeatedChats());

            yield return null;

            while (!this.chatManager.GetIsClear())
                yield return null;

            this.battleSystem.EndBattle(true);
        }
    }
}