#region Libraries

using System.Collections.Generic;
using Runtime.Player;
using Runtime.UI.Communication;
using Runtime.UI.TextEffects;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.Communication
{
    [CreateAssetMenu(fileName = "Chat", menuName = "Chat/Create new Standard Chat", order = 0)]
    public class Chat : ScriptableObject
    {
        #region Values

        [SerializeField, Required] protected ChatManager chatManager;
        [SerializeField, Required] private PlayerManager playerManager;

        [Header("Object Reference:")] [SerializeField]
        protected bool isInstantiated;

        [SerializeField] protected string location;
        [SerializeField, TextArea] protected string description;
        [SerializeField, TextArea] protected List<string> textList;
        [SerializeField] protected List<float> timeToNext;
        [SerializeField] protected List<float> textBetweenTimes;

        [SerializeField] protected bool hideAfter;

        [Header("Continuation:")] [SerializeField]
        protected bool needInput = true;

        [Header("Overriding:")] [SerializeField]
        protected List<string> replaceString = new List<string>();

        [SerializeField] protected List<string> addString = new List<string>();

        [Header("Text Animation:")] [SerializeField]
        protected string showText;

        private int index;
        private bool active;
        protected bool waiting, done;
        protected int nextCharacter;

        #endregion

        #region Build In States

        private void OnValidate()
        {
            this.textList ??= new List<string>();
            this.timeToNext ??= new List<float>();

            if (this.textList.Count > this.timeToNext.Count)
            {
                List<float> arr = new List<float>(this.textList.Count);

                for (int i = 0; i < this.timeToNext.Count; i++)
                    arr[i] = this.timeToNext[i];

                this.timeToNext = arr;
            }
            else if (this.textList.Count < this.timeToNext.Count)
            {
                List<float> arr = new List<float>(this.textList.Count);

                for (int i = 0; i < this.textList.Count; i++)
                    arr[i] = this.timeToNext[i];

                this.timeToNext = arr;
            }
        }

        #endregion

        #region Getters

        public Chat GetChatInstantiated()
        {
            Chat result = this;

            if (this.isInstantiated) return result;

            result = Instantiate(result);
            result.SetIsInstantiated();

            return result;
        }

        public bool GetDone() =>
            this.done;

        public bool GetNeedInput() =>
            this.needInput;

        public int GetListCount =>
            this.textList.Count;

        public string GetTextByIndex(int i) =>
            this.textList[i];

        public float GetTimeToNextByIndex(int i) =>
            this.timeToNext[i];

        public bool GetHideAfter() =>
            this.hideAfter;

        #endregion

        #region Setters

        private void SetIsInstantiated() =>
            this.isInstantiated = true;

#if UNITY_EDITOR
        public void SetTextByIndex(int i, string set) => this.textList[i] = set;

        public void SetTimeToNextByIndex(int i, float set) => this.timeToNext[i] = set;
#endif

        #endregion

        #region In

#if UNITY_EDITOR
        public void CreateNew()
        {
            int length = this.textList.Count + 1;
            List<string> sArr = new List<string>(length);
            List<float> fArr = new List<float>(length);

            for (int j = 0; j < this.textList.Count; j++)
            {
                sArr[j] = this.textList[j];
                fArr[j] = this.timeToNext[j];
            }

            this.textList = sArr;
            this.timeToNext = fArr;
        }

        public void DeleteByIndex(int i)
        {
            int length = this.textList.Count - 1;
            List<string> sArr = new List<string>(length);
            List<float> fArr = new List<float>(length);

            int offset = 0;

            for (int j = 0; j < this.textList.Count; j++)
            {
                if (j == i)
                {
                    offset = -1;
                    continue;
                }

                sArr[j + offset] = this.textList[j];
                fArr[j + offset] = this.timeToNext[j];
            }

            this.textList = sArr;
            this.timeToNext = fArr;
        }
#endif

        public void AddToOverride(string replace, string add)
        {
            this.replaceString.Add(replace);
            this.addString.Add(add);
        }

        #endregion

        #region Out

        public TextEffectBase Play(TextField textField)
        {
            this.Setup();
            return textField.GetText.ShowOverTime(this.textBetweenTimes[this.index], () => this.waiting = true);
        }

        public TextEffectBase PlayNext(TextField textField)
        {
            this.IncreaseIndex();

            this.done = false;

            return this.Play(textField);
        }

        #endregion

        #region Internal

        protected virtual void Setup()
        {
            if (this.active) return;

            this.index = 0;
            this.CheckTextOverride();
            this.active = true;
            this.chatManager.ShowTextField(true);
        }

        protected virtual void OnFinalDone() =>
            this.done = true;

        private void IncreaseIndex()
        {
            this.index++;
            this.waiting = false;
        }

        private void CheckTextOverride()
        {
            this.AddPlayerInfoToOverride();

            for (int i = 0; i < this.textList.Count; i++)
            {
                for (int j = 0; j < this.replaceString.Count; j++)
                {
                    if (j < this.replaceString.Count && j < this.addString.Count)
                        this.textList[i] = this.textList[i].Replace(this.replaceString[j], this.addString[j]);
                }
            }
        }

        private void AddPlayerInfoToOverride()
        {
            string[] pronouns = this.playerManager.GetPronouns();

            this.AddToOverride("P_NAME", this.playerManager.name);
            this.AddToOverride("P_ONE", pronouns[0]);
            this.AddToOverride("P_TWO", pronouns[1]);
        }

        #endregion
    }
}