#region Packages

using System;
using System.Collections;
using System.Collections.Generic;
using Runtime.Communication;
using UnityEngine;

#endregion

namespace Runtime.AI.Battle.AfterEffects
{
    [Serializable]
    public class TriggerAfterChat : NpcBattleAfterEffect
    {
        [SerializeField] private ChatManager chatManager;
        [SerializeField] private Chat chat;

        private Dictionary<string, string> input;

        #region In

        public override void SetInput(object i)
        {
            try
            {
                this.input = i as Dictionary<string, string>;
            }
            catch (Exception e)
            {
                Debug.LogError("Input was not Dictionary of string/string\n" + e.Message);
                this.input = null;
            }
        }

        public override IEnumerator Operation()
        {
            if (this.input == null) yield break;

            // ReSharper disable once AccessToStaticMemberViaDerivedType
            this.chat = Instantiate(this.chat);
            foreach (string inputKey in this.input.Keys)
                this.chat.AddToOverride(
                    inputKey, this.input[inputKey]);

            this.chatManager.Add(this.chat);

            yield return new WaitWhile(() =>
                !this.chatManager.GetIsClear());
            this.done = true;
        }

        #endregion

        #region Out

        public override NpcBattleAfterEffect GetInstantiatedEffect()
        {
            NpcBattleAfterEffect effect = Instantiate(this);
            effect.IsInstantiated();
            return effect;
        }

        #endregion
    }
}