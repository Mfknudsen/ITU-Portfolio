#region Packages

using System.Collections;
using Runtime.Communication;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.Battle.Systems.BattleStart
{
    public sealed class AskBattleStarter : BattleStarter
    {
        #region Values

        [SerializeField, Required] private QuestionChat askBattleChat;

        [SerializeField, Required] private Chat deniedChat;

        #endregion

        #region In

        public override void InteractTrigger()
        {
            this.unitManager.PauseAllUnits();
            this.playerManager.DisablePlayerControl();

            QuestionChat instantiatedChat = (QuestionChat)this.askBattleChat.GetChatInstantiated();
            instantiatedChat.AddToOverride("<TRAINER_NAME>", this.enemies[0].GetName());
            for (int i = 0; i < this.enemies.Length; i++)
                instantiatedChat.AddToOverride($"<TRAINER_NAME_{i}>", this.enemies[i].GetName());

            instantiatedChat.AddResponse("Yes", this.TriggerBattle);
            instantiatedChat.AddResponse("No", () => this.StartCoroutine(this.DeniedResponse()));

            this.chatManager.Add(instantiatedChat);
        }

        #endregion

        #region Internal

        private IEnumerator DeniedResponse()
        {
            Chat instantiatedChat = this.deniedChat.GetChatInstantiated();

            instantiatedChat.AddToOverride("<TRAINER_NAME>", this.enemies[0].GetName());
            for (int i = 0; i < this.enemies.Length; i++)
                instantiatedChat.AddToOverride($"<TRAINER_NAME_{i}>", this.enemies[i].GetName());

            this.chatManager.Add(instantiatedChat);

            yield return new WaitWhile(() => !instantiatedChat.GetDone() || !this.chatManager.GetIsClear());

            this.unitManager.ResumeAllUnits();
            this.playerManager.EnablePlayerControl();
        }

        #endregion
    }
}