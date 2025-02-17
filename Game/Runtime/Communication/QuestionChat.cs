#region Packages

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#endregion

namespace Runtime.Communication
{
    [CreateAssetMenu(fileName = "Question Chat", menuName = "Chat/Create new Question Chat")]
    public sealed class QuestionChat : Chat
    {
        #region Values

        [SerializeField] private List<string> labels = new List<string>();
        [SerializeField] private List<UnityEvent> actions = new List<UnityEvent>();

        #endregion

        #region In

        public void AddResponse(string label, UnityAction action)
        {
            UnityEvent unityEvent = new UnityEvent();
            unityEvent.AddListener(action);

            this.labels.Add(label);
            this.actions.Add(unityEvent);
        }

        public void AddResponse(string label, UnityEvent action)
        {
            this.labels.Add(label);
            this.actions.Add(action);
        }

        #endregion

        #region Internal

        protected override void Setup()
        {
            base.Setup();

            this.needInput = true;
        }

        protected override void OnFinalDone()
        {
            if (this.done)
                return;

            this.needInput = true;
            this.done = true;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            UnityEvent[] arr = new UnityEvent[this.labels.Count];
            for (int i = 0; i < this.labels.Count; i++)
            {
                int responseIndex = i;
                UnityEvent e = new UnityEvent();
                e.AddListener(() => this.Response(responseIndex));
                arr[i] = e;
            }

            this.chatManager.ShowResponses(this.labels.ToArray(), arr);
        }

        private void Response(int i)
        {
            this.done = true;
            this.needInput = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            this.actions[i].Invoke();
        }

        #endregion
    }
}