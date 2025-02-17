#region Packages

using System.Collections;
using Runtime.Communication;
using Runtime.Pokémon;
using Runtime.Pokémon.Conditions.Non_Volatiles;
using UnityEngine;

#endregion

namespace Runtime.Items.Medicine
{
    [CreateAssetMenu(fileName = "Item", menuName = "Item/Create new Revive")]
    public class Revive : BattleItem
    {
        #region Values

        [SerializeField] private bool toFull;
        [SerializeField] private Chat onActivation;

        #endregion

        #region Overrides

        public override void SetTarget(Pokemon set)
        {
            base.SetTarget(set);

            set.SetRevived(true);
        }

        public override bool IsUsableTarget(Pokemon p)
        {
            if (p.GetConditionOversight().GetNonVolatileStatus() is null) return false;

            return p.GetConditionOversight().GetNonVolatileStatus() is FaintedCondition && !p.GetRevived();
        }

        public override IEnumerator Operation()
        {
            this.done = false;

            Chat toSend = this.onActivation.GetChatInstantiated();
            toSend.AddToOverride("<POKEMON_NAME>", this.target.GetName());
            this.chatManager.Add(toSend);

            this.target.GetConditionOversight().TryApplyNonVolatileCondition(null);

            if (this.toFull)
                this.target.ReceiveDamage(-Mathf.Infinity);
            else
                this.target.ReceiveDamage(-(this.target.GetCalculatedStat(Stat.HP) / 2));

            while (!this.chatManager.GetIsClear())
                yield return null;

            this.done = true;
        }

        #endregion
    }
}