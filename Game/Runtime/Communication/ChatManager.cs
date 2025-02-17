#region Libraries

using System.Collections;
using System.Collections.Generic;
using Runtime.Systems;
using Runtime.Systems.PersistantRunner;
using Runtime.UI.Communication;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

#endregion

namespace Runtime.Communication
{
    [CreateAssetMenu(menuName = "Manager/Chat")]
    public sealed class ChatManager : Manager, IFrameStart, IFrameUpdate
    {
        #region Values

        [ShowInInspector, ReadOnly] private TextField textField;

        [ShowInInspector, ReadOnly] private Chat running;

        [ShowInInspector, ReadOnly] private List<Chat> waitList = new List<Chat>();

        [ShowInInspector, ReadOnly] private TextMeshProUGUI text;

        [SerializeField] private bool waitForInput = true;

        [SerializeField] private int completeTime = 30;

        private bool show, adaptiveText;

        #endregion

        #region Getters

        public bool GetIsClear() =>
            this.running == null && this.waitList.Count == 0;

        public float GetTextSpeed() =>
            1f / this.completeTime;

        public bool GetShow() =>
            this.show;

        #endregion

        #region Setters

        public void SetDisplayText(string set) =>
            this.text.text = set;

        public void ShowTextField(bool set)
        {
            this.show = set;

            if (this.textField == null) return;

            if (set)
                this.textField.Show();
            else
                this.textField.Hide();
        }

        public void SetAdaptive(bool set) => this.adaptiveText = set;

        public void SetTextfield(TextField set) => this.textField = set;

        #endregion

        #region In

        public IEnumerator FrameStart(PersistantRunner runner)
        {
            this.running = null;
            this.text = null;
            this.waitList = new List<Chat>();

            InputManager.Instance.nextChatInputEvent.AddListener(this.OnNextChatChange);

            this.ready = true;
            yield break;
        }

        public void FrameUpdate()
        {
            if (this.textField == null) return;

            this.text = this.textField.GetText;

            if (this.text == null) return;

            if (this.running != null && this.waitForInput)
            {
                if (this.running.GetNeedInput()) return;

                if (this.running.GetDone())
                    this.CleanDone();
                else
                    this.running.PlayNext(this.textField);

                this.waitForInput = false;
            }
            else if (this.running == null && this.waitList.Count > 0)
                this.PlayNextInLine();

            if (!this.show || !this.adaptiveText) return;

            if (this.running == null)
                this.textField.Hide();
            else
                this.textField.Show();
        }

        public void Add(IEnumerable<Chat> toAdd)
        {
            foreach (Chat c in toAdd)
                this.waitList.Add(c.GetChatInstantiated());
        }

        public void Add(Chat toAdd) =>
            this.waitList.Add(toAdd.GetChatInstantiated());

        public void ShowResponses(string[] labels, UnityEvent[] actions)
        {
            if (this.textField == null) return;

            this.textField.GetResponseField.Show(labels, actions);
        }

        #endregion

        #region Internal

        private void OnNextChatChange()
        {
            if (this.running is QuestionChat) return;

            if (this.running == null || !this.running.GetNeedInput() || !this.waitForInput) return;

            if (this.running.GetDone())
                this.CleanDone();
            else
                this.running.PlayNext(this.textField);

            this.waitForInput = false;
        }

        public void CheckRunningState() =>
            this.waitForInput = true;

        private void PlayNextInLine()
        {
            this.running = this.waitList[0];
            this.waitList.RemoveAt(0);

            this.Play(this.running);
            this.ShowTextField(true);
        }

        private void Play(Chat toPlay)
        {
            this.text.gameObject.SetActive(true);

            this.running = toPlay;
            this.running.Play(this.textField);
        }

        private void CleanDone()
        {
            if (this.running == null) return;

            Destroy(this.running);

            this.running = null;
        }

        #endregion
    }
}