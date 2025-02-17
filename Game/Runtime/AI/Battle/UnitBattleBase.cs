#region Packages

using System;
using System.Collections;
using System.Collections.Generic;
using Runtime.AI.Battle.AfterEffects;
using Runtime.AI.Battle.BeforeEffects;
using Runtime.Battle.Systems.BattleStart;
using Runtime.Communication;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

#endregion

namespace Runtime.AI.Battle
{
    public class UnitBattleBase : UnitBase
    {
        #region Values

        [SerializeField, Required] private ChatManager chatManager;

        [SerializeField, FoldoutGroup("Base")] private Chat idleChat;

        [FoldoutGroup("Before Battle")] [SerializeField]
        private BattleStarter battleStarter;

        [FoldoutGroup("Before Battle")] [SerializeField]
        private BeforeEffectContainer[] beforeEffects = Array.Empty<BeforeEffectContainer>();

        [FoldoutGroup("Before Battle")] [SerializeField]
        private AfterEffectContainer[] afterEffects = Array.Empty<AfterEffectContainer>();

        [FoldoutGroup("Input")] [SerializeField]
        private string[] chatKeys, chatValues;

        #endregion

        #region Build In States

        private void OnValidate()
        {
            this.beforeEffects.ForEach(e =>
                e.OnValidate());
            this.afterEffects.ForEach(e =>
                e.OnValidate());
        }

        private void Start()
        {
            //Setup Input
            Dictionary<string, string> chatInput = this.SetupChatInput();

            //Apply
            foreach (BeforeEffectContainer container in this.beforeEffects)
                container.SetInput<TriggerBeforeChat>(chatInput);

            foreach (AfterEffectContainer container in this.afterEffects)
                container.SetInput<TriggerAfterChat>(chatInput);

            this.battleStarter ??= this.GetComponent<BattleStarter>();
        }

        #endregion

        #region In

        public override void InteractTrigger()
        {
            if (this.battleStarter.GetPlayerWon())
                this.chatManager.Add(this.idleChat);
            else
                this.StartCoroutine(this.BeforeBattle());
        }

        #endregion

        #region Internal

        #region Setup Input

        private Dictionary<string, string> SetupChatInput()
        {
            if (this.chatKeys == null)
                return null;

            Dictionary<string, string> chatInput = new Dictionary<string, string>();
            for (int i = 0; i < this.chatKeys.Length && i < this.chatValues.Length; i++)
                chatInput.Add(this.chatKeys[i], this.chatValues[i]);

            return chatInput;
        }

        #endregion

        private IEnumerator BeforeBattle()
        {
            foreach (BeforeEffectContainer container in this.beforeEffects)
            {
                container.Trigger();

                yield return null;

                yield return new WaitWhile(() => !container.AllDone());
            }

            this.battleStarter.InteractTrigger();
        }

        public IEnumerator AfterBattle()
        {
            foreach (AfterEffectContainer container in this.afterEffects)
            {
                container.Trigger();

                yield return null;

                yield return new WaitWhile(() => !container.AllDone());
            }
        }

        #endregion
    }
}