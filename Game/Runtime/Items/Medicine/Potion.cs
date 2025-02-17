#region Packages

using System.Collections;
using Runtime.Battle.Systems;
using Runtime.Communication;
using Runtime.Pokémon;
using Runtime.Pokémon.Conditions.Non_Volatiles;
using UnityEngine;

#endregion

namespace Runtime.Items.Medicine
{
    [CreateAssetMenu(fileName = "Item", menuName = "Item/Create new Potion")]
    public class Potion : BattleItem
    {
        #region Values

        [SerializeField] private float healAmount;
        [SerializeField] private Chat onActivation;

        #endregion

        #region Overrides

        public override bool IsUsableTarget(Pokemon p)
        {
            if (p.GetConditionOversight().GetNonVolatileStatus() != null)
            {
                if (!(p.GetConditionOversight().GetNonVolatileStatus() is FaintedCondition))
                    return true;
            }
            else
                return true;

            Debug.Log("Error");
            return false;
        }

        public override IEnumerator Operation()
        {
            this.done = false;
            float curHeal = 0, healSpeed = this.healAmount / 200 * BattleSystem.instance.GetSecPerPokeMove();

            while (curHeal < this.healAmount)
            {
                if (curHeal + healSpeed < this.healAmount)
                {
                    this.target.ReceiveDamage(-healSpeed);
                    curHeal += healSpeed;
                    yield return new WaitForSeconds(BattleSystem.instance.GetSecPerPokeMove() /
                                                    (200 * BattleSystem.instance.GetSecPerPokeMove()));
                }
                else
                {
                    this.target.ReceiveDamage(-(this.healAmount - curHeal));
                    curHeal += healSpeed;
                    yield return null;
                }
            }

            Chat toSend = this.onActivation.GetChatInstantiated();
            toSend.AddToOverride("<POKEMON_NAME>", this.target.GetName());
            this.chatManager.Add(toSend);

            while (!this.chatManager.GetIsClear())
                yield return null;

            this.done = true;
        }

        #endregion
    }
}