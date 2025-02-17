#region Packages

using System.Collections;
using Runtime.Battle.Systems;
using Runtime.Communication;
using Runtime.Systems;
using UnityEngine;

#endregion

namespace Runtime.Pokémon.Conditions.Non_Volatiles
{
    [CreateAssetMenu(fileName = "Condition", menuName = "Condition/Create new Non-Volatile Condition/Fainted",
        order = 0)]
    public sealed class FaintedCondition : NonVolatileCondition, IOperation
    {
        #region Values

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

        #endregion

        #region In

        public override void Reset()
        {
            this.done = false;
        }

        public bool IsOperationDone => this.done;

        public IEnumerator Operation ()
        {
            Chat toSend = this.onEffectChat.GetChatInstantiated();
            toSend.AddToOverride("<POKEMON_NAME>", this.affectedPokemon.GetName());
            this.chatManager.Add(toSend);

            GameObject obj = this.affectedPokemon.GetSpawnedObject();

            while (!this.chatManager.GetIsClear())
            {
                if (obj.transform.localScale.y > 0.01f)
                    obj.transform.localScale += -Vector3.one * Time.deltaTime;

                yield return null;
            }

            BattleSystem.instance.DespawnPokemon(this.affectedPokemon);

            yield return new WaitForSeconds(1);

            this.done = true;
        }

        public void OperationEnd()
        {
        }

        #endregion
    }
}