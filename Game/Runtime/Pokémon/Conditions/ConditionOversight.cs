#region Packages

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Runtime.Battle.Systems;
using Runtime.Communication;
using Runtime.Pokémon.Conditions.Non_Volatiles;
using Runtime.Systems;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

// ReSharper disable ParameterHidesMember
namespace Runtime.Pokémon.Conditions
{
    [CreateAssetMenu(fileName = "ConditionOversight", menuName = "Condition/Create new Condition Oversight")]
    public class ConditionOversight : ScriptableObject
    {
        #region Values

        [SerializeField, Required] private OperationManager operationManager;
        [SerializeField, Required] private ChatManager chatManager;

        [SerializeField]
        private Condition nonVolatileStatus; //Burn, Freeze, Paralysis, Poison, Sleep. Only one can be active.

        [SerializeField] private List<VolatileCondition>
            volatileStatus = new List<VolatileCondition>(); //Bound, CantEscape, Confusion, Curse...

        [SerializeField] private bool done, isStunned;

        private Pokemon pokemon;

        #endregion

        #region Getters

        public Condition GetNonVolatileStatus()
        {
            return this.nonVolatileStatus;
        }

        public bool GetDone()
        {
            return this.done;
        }

        public bool GetIsStunned()
        {
            return this.isStunned;
        }

        #endregion

        #region Setters

        public void SetIsStunned(bool set)
        {
            this.isStunned = set;
        }

        #endregion

        #region In

        public void Setup(Pokemon pokemon)
        {
            this.volatileStatus = new List<VolatileCondition>();
            this.pokemon = pokemon;
        }

        public void Reset()
        {
            this.done = false;
        }

        public void ApplyVolatileCondition(VolatileCondition condition)
        {
            if (this.volatileStatus.Any(volatileCondition =>
                volatileCondition.GetConditionName().Equals(condition.GetConditionName()))) return;

            this.volatileStatus.Add(condition);

            foreach (Ability ability in BattleSystem.instance.GetAbilityOversight().GetAbilities())
            {
                if (ability.GetActive())
                    ability.TriggerDisable(AbilityTrigger.OnStatusChange, this.pokemon);
                else
                    ability.TriggerEnable(AbilityTrigger.OnStatusChange, this.pokemon);
            }
        }

        public void TryApplyNonVolatileCondition(NonVolatileCondition iNonVolatileCondition)
        {
            Condition condition = iNonVolatileCondition;

            if (condition is FaintedCondition)
            {
                Destroy(this.nonVolatileStatus);

                this.nonVolatileStatus = condition;

                this.nonVolatileStatus.SetAffectedPokemon(this.pokemon);
            }
            else
            {
                if (!(condition is null) && this.nonVolatileStatus is null)
                {
                    Destroy(this.nonVolatileStatus);
                    this.nonVolatileStatus = condition.GetCondition();
                    this.nonVolatileStatus.SetAffectedPokemon(this.pokemon);
                }
                else if (condition is null && !(this.nonVolatileStatus is null))
                {
                    if (!(this.nonVolatileStatus is FaintedCondition)) return;

                    Destroy(this.nonVolatileStatus);
                    this.nonVolatileStatus = null;
                }
            }

            foreach (Ability ability in BattleSystem.instance.GetAbilityOversight().GetAbilities())
            {
                if (ability.GetActive())
                    ability.TriggerDisable(AbilityTrigger.OnStatusChange, this.pokemon);
                else
                    ability.TriggerEnable(AbilityTrigger.OnStatusChange, this.pokemon);
            }
        }

        public void RemoveFromCondition(Condition condition)
        {
            if (this.volatileStatus.Contains(condition))
            {
                this.volatileStatus.Remove(condition as VolatileCondition);
            }
            else if (this.nonVolatileStatus == condition)
            {
                this.nonVolatileStatus = null;
            }
        }

        public void ResetConditionList()
        {
            this.volatileStatus.Clear();
        }

        #endregion

        #region Out

        public IEnumerator CheckConditionBeforeMove()
        {
            this.done = false;

            #region Check To Play

            List<Condition> toPlay = new List<Condition>();
            if (this.nonVolatileStatus != null)
            {
                if (!this.nonVolatileStatus.GetBeforeAttack())
                    toPlay.Add(this.nonVolatileStatus);
            }

            toPlay.AddRange(this.volatileStatus
                .Where(volatileStatus => !volatileStatus.GetBeforeAttack()));

            #endregion

            #region Play All

            foreach (Condition condition in toPlay)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (!(condition is IOperation iOperation)) continue;

                OperationsContainer container = new OperationsContainer();
                container.Add(iOperation);
                this.operationManager.AddOperationsContainer(container);

                while (!this.operationManager.GetDone() || !this.chatManager.GetIsClear())
                    yield return null;

                condition.Reset();
            }

            #endregion

            this.done = true;
        }

        public IEnumerator CheckConditionEndTurn()
        {
            this.done = false;

            List<Condition> toPlay = new List<Condition>();

            #region Check To Play

            if (!(this.nonVolatileStatus is null))
            {
                if (!this.nonVolatileStatus.GetBeforeAttack())
                    toPlay.Add(this.nonVolatileStatus.GetCondition());
            }

            toPlay.AddRange(this.volatileStatus
                .Where(volatileStatus => !toPlay.Contains(volatileStatus)));

            #endregion

            #region Play All

            foreach (Condition condition in toPlay)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (condition is not IOperation iOperation) continue;

                OperationsContainer container = new OperationsContainer();
                container.Add(iOperation);
                this.operationManager.AddOperationsContainer(container);

                while (!this.operationManager.GetDone() || !this.chatManager.GetIsClear())
                    yield return null;

                condition.Reset();
            }

            #endregion

            this.done = true;
        }

        public void CheckFaintedCondition()
        {
            this.done = false;
            Condition condition = this.nonVolatileStatus;

            if (condition is FaintedCondition and IOperation iOperation)
            {
                OperationsContainer container = new OperationsContainer();
                container.Add(iOperation);
                this.operationManager.AddOperationsContainer(container);
            }

            this.done = true;
        }

        #endregion
    }
}