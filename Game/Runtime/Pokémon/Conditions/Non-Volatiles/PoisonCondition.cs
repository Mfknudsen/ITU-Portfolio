#region Packages

using System.Collections;
using Runtime.Battle.Systems;
using Runtime.Communication;
using Runtime.Systems;
using UnityEngine;

#endregion

namespace Runtime.Pokémon.Conditions.Non_Volatiles
{
    [CreateAssetMenu(fileName = "Condition", menuName = "Condition/Create new Non-Volatile Condition/Poison",
        order = 1)]
    public class PoisonCondition : NonVolatileCondition, IOperation
    {
        #region Values

        [SerializeField] private bool badlyPoison;
        [SerializeField] private float damage;
        [SerializeField] private float n, increaseN = 1;
        [SerializeField] private Chat onEffectChat;

        private bool done;
        
        #endregion

        #region Getters

        public override Condition GetCondition()
        {
            Condition result = this;

            if (!result.GetIsInstantiated())
            {
                result = Instantiate(this);
                result.SetIsInstantiated(true);
            }

            return result;
        }

        public float GetDamage()
        {
            float result = this.damage * this.n;

            if (this.badlyPoison) this.n += this.increaseN;

            return result;
        }

        #endregion

        #region Setters

        public void SetDamage(int maxHP)
        {
            this.damage = maxHP / 16;
        }

        public void SetBadlyPoison(bool set)
        {
            this.badlyPoison = set;
        }

        #endregion

        #region In

        public override void Reset()
        {
            this.done = false;
        }

        public bool IsOperationDone => this.done;

        public IEnumerator Operation()
        {
            Chat toSend = this.onEffectChat.GetChatInstantiated();
            toSend.AddToOverride("<TARGET_NAME>", this.affectedPokemon.GetName());
            this.chatManager.Add(toSend);

            if (this.damage == 0) this.SetDamage(this.affectedPokemon.GetCalculatedStat(Stat.HP));

            this.damage = this.GetDamage();
            float divide = 200;
            float reletivSpeed = BattleSystem.instance.GetSecPerPokeMove() / divide;
            float relativeDamage = this.damage / divide;
            float appliedDamage = 0;

            while (appliedDamage < this.damage)
            {
                appliedDamage += relativeDamage;

                this.affectedPokemon.ReceiveDamage(relativeDamage);

                yield return new WaitForSeconds(reletivSpeed);
            }

            this.done = true;
        }

        public void OperationEnd()
        {
        }

        #endregion
    }
}