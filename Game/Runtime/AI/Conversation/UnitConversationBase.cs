#region Packages

using System.Collections;
using Runtime.Communication;
using Runtime.Player;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.AI.Conversation
{
    public class UnitConversationBase : UnitBase
    {
        #region Values

        [SerializeField] private string unitName;
        [SerializeField, Required] private Chat chatOnInteraction;
        [SerializeField, Required] private ChatManager chatManager;
        [SerializeField, Required] private PlayerManager playerManager;

        private bool chatting;

        #endregion

        #region In

        public override void InteractTrigger()
        {
            if (this.chatting) return;

            this.chatting = true;

            this.playerManager.DisablePlayerControl();

            this.StartCoroutine(this.StartChat());
        }

        #endregion

        #region Internal

        private IEnumerator StartChat()
        {
            Chat instantiatedChat = this.chatOnInteraction.GetChatInstantiated();
            instantiatedChat.AddToOverride("<UNIT_NAME>", this.unitName);

            this.chatManager.Add(instantiatedChat);

            yield return new WaitWhile(() => this.chatManager.GetIsClear());

            this.chatting = false;

            this.playerManager.EnablePlayerControl();
        }

        #endregion
    }
}