#region Packages

using System.Collections.Generic;
using Runtime.World.Overworld.Interactions;
using UnityEngine;
using UnityEngine.Events;

#endregion

namespace Runtime.Communication
{
    public sealed class QuestionChatHandler : MonoBehaviour, IInteractable
    {
        #region Values

        [SerializeField] private QuestionChat questionChat;

        [SerializeField] private List<UnityEvent> actions = new List<UnityEvent>();
        [SerializeField] private List<string> labels = new List<string>();

        #endregion

        #region Build In States

        private void OnValidate()
        {
            if (this.actions.Count >= this.labels.Count) return;

            for (int i = 0; i < this.labels.Count - this.actions.Count; i++)
                this.actions.Add(null);
        }

        #endregion

        #region In

        public void InteractTrigger()
        {
            for (int i = 0; i < this.labels.Count; i++)
                this.questionChat.AddResponse(this.labels[i], this.actions[i]);
        }

        #endregion

        #region Out

        public QuestionChat Get()
        {
            QuestionChat instantiatedChat = (QuestionChat)this.questionChat.GetChatInstantiated();

            for (int i = 0; i < this.labels.Count; i++)
                instantiatedChat.AddResponse(this.labels[i], this.actions[i]);

            return instantiatedChat;
        }

        #endregion
    }
}