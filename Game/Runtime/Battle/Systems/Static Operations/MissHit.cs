#region Packages

using System.Collections;
using Runtime.Communication;
using Runtime.Pokémon;
using Runtime.Systems;
using UnityEngine;

#endregion

namespace Runtime.Battle.Systems.Static_Operations
{
    public sealed class MissHit : IOperation
    {
        private readonly ChatManager chatManager;
        private readonly Chat missChat;
        private readonly Pokemon targetPokemon;
        private bool done;

        public MissHit(Pokemon targetPokemon, ChatManager chatManager)
        {
            this.targetPokemon = targetPokemon;

            this.missChat = Object.Instantiate(BattleMathf.GetMissChat());
            this.chatManager = chatManager;
        }

        public bool IsOperationDone => this.done;

        public IEnumerator Operation()
        {
            this.done = false;

            this.missChat.AddToOverride("<POKEMON_NAME>", this.targetPokemon.GetName());

            this.chatManager.Add(this.missChat);

            while (!this.chatManager.GetIsClear())
                yield return null;

            this.done = true;
        }

        public void OperationEnd()
        {
        }
    }
}