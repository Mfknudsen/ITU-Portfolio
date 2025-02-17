#region Packages

using System.Collections;
using Runtime.Communication;
using Runtime.Systems;
using UnityEngine;

#endregion

namespace Runtime.Pokémon.Conditions.Non_Volatiles
{
    [CreateAssetMenu(fileName = "Condition", menuName = "Condition/Create new Non-Volatile Condition/Freeze",
        order = 1)]
    public class FreezeCondition : NonVolatileCondition, IOperation
    {
        #region Values

        [SerializeField] private int n;

        [SerializeField] private Chat onEffectChat,
            onContinuousEffectChat,
            abruptEndChat,
            endEffectChat;

        private bool done;

        #endregion

        #region Getters

        public override Condition GetCondition()
        {
            Condition result = this;

            if (result.GetIsInstantiated()) return result;

            result = Instantiate(this);
            result.SetIsInstantiated(true);

            return result;
        }

        #endregion

        #region In

        public override void Reset()
        {
            this.done = false;
        }

        public IEnumerator Operation()
        {
            this.n++;

            if ((Random.Range(0.0f, 1.0f) <= 0.2f) && (this.n != 5))
            {
                Chat toSend = this.abruptEndChat.GetChatInstantiated();
                toSend.AddToOverride("<POKEMON_NAME>", this.affectedPokemon.GetName());
                this.chatManager.Add(toSend);

                this.conditionOversight.RemoveFromCondition(this);
            }
            else if (this.n < 5)
            {
                Chat toSend = this.onContinuousEffectChat.GetChatInstantiated();
                toSend.AddToOverride("<POKEMON_NAME>", this.affectedPokemon.GetName());
                this.chatManager.Add(toSend);

                this.conditionOversight.SetIsStunned(true);
            }
            else
            {
                Chat toSend = this.endEffectChat.GetChatInstantiated();
                toSend.AddToOverride("<POKEMON_NAME>", this.affectedPokemon.GetName());
                this.chatManager.Add(this.endEffectChat);

                this.conditionOversight.RemoveFromCondition(this);
            }

            yield return null;

            this.done = true;
        }

        public void OperationEnd()
        {
        }

        #endregion

        public bool IsOperationDone => this.done;
    }
}