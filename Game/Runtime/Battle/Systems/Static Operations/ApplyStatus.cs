using System.Collections;
using Runtime.Communication;
using Runtime.Pokémon;
using Runtime.Pokémon.Conditions;
using Runtime.Systems;

namespace Runtime.Battle.Systems.Static_Operations
{
    public class ApplyStatus : IOperation
    {
        private readonly ChatManager chatManager;
        private bool done;
        private readonly Chat statusChat;
        private readonly Pokemon targetPokemon;
        private readonly Condition statusCondition;

        public ApplyStatus(Chat statusChat, Pokemon targetPokemon, Condition statusCondition)
        {
            this.statusChat = statusChat.GetChatInstantiated();
            this.targetPokemon = targetPokemon;
            this.statusCondition = statusCondition;
        }

        public bool IsOperationDone => this.done;

        public IEnumerator Operation()
        {
            this.done = false;

            this.statusChat.AddToOverride("<TARGET_NAME>", this.targetPokemon.GetName());

            this.chatManager.Add(this.statusChat);

            if (this.statusCondition is NonVolatileCondition nonVolatile)
                this.targetPokemon.GetConditionOversight()
                    .TryApplyNonVolatileCondition(nonVolatile);
            else
                this.targetPokemon.GetConditionOversight().ApplyVolatileCondition((VolatileCondition)this.statusCondition);


            while (!this.chatManager.GetIsClear())
                yield return null;

            this.done = true;
        }

        public void OperationEnd()
        {
        }
    }
}