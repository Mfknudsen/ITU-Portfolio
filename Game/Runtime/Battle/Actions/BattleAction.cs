#region Packages

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Runtime.AI.Battle.Evaluator;
using Runtime.AI.Battle.Evaluator.Virtual;
using Runtime.Battle.Systems;
using Runtime.Battle.Systems.Spots;
using Runtime.Communication;
using Runtime.Player;
using Runtime.Pokémon;
using Runtime.Systems;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.Battle.Actions
{
    public abstract class BattleAction : ScriptableObject, IOperation
    {
        #region Values

        [SerializeField, Required] protected PlayerManager playerManager;
        [SerializeField, Required] protected OperationManager operationManager;
        [SerializeField, Required] protected ChatManager chatManager;
        [SerializeField] protected bool active, done;
        [SerializeField] private bool defaultTargetEnemy = true;

        [SerializeField] protected Pokemon currentPokemon;

        [SerializeField] protected List<Spot> targets;
        [SerializeField] protected bool moveActive;

        [SerializeField] protected int priority;
        [SerializeField] protected string[] priorityInteraction;
        [SerializeField] protected Chat[] chatOnActivation = Array.Empty<Chat>();

        private bool isInstantiated;

        #endregion

        #region Getters

        public virtual BattleAction GetAction()
        {
            return this;
        }

        public Pokemon GetCurrentPokemon()
        {
            return this.currentPokemon;
        }

        public bool GetIsInstantiated()
        {
            return this.isInstantiated;
        }

        public int GetPriority()
        {
            return this.priority;
        }

        public bool GetDefaultTargetEnemy()
        {
            return this.defaultTargetEnemy;
        }

        #endregion

        #region Setter

        public void SetTargets(Pokemon pokemon)
        {
            this.targets ??= new List<Spot>();

            this.targets.Add(BattleSystem.instance.GetSpotOversight().GetSpots()
                .FirstOrDefault(s => s.GetActivePokemon() == pokemon));
        }

        public void SetCurrentPokemon(Pokemon pokemon)
        {
            this.currentPokemon = pokemon;
        }

        public void SetIsInstantiated(bool set)
        {
            this.isInstantiated = set;
        }

        public void SetPriority(int set)
        {
            this.priority = set;
        }

        #endregion

        #region Out

        public abstract float Evaluate(Pokemon user, Pokemon target, VirtualBattle virtualBattle,
            PersonalitySetting personalitySetting);

        #endregion

        #region Internal

        protected void SetupChats()
        {
            for (int i = 0; i < this.chatOnActivation.Length; i++) this.chatOnActivation[i] = Instantiate(this.chatOnActivation[i]);
        }

        protected virtual Chat[] TransferInformationToChat()
        {
            Debug.Log("Transferring");
            return Array.Empty<Chat>();
        }

        #endregion

        #region IOperation

        public bool IsOperationDone => this.done;

        public virtual IEnumerator Operation()
        {
            Debug.Log("Operation need Override!");
            yield return null;
        }

        public void OperationEnd()
        {
        }

        #endregion
    }
}