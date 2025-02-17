#region Libraries

using Runtime.Pokémon;
using Logger = Runtime.Testing.Logger;

#endregion

namespace Runtime.AI
{
    #region Enums

    public enum PokemonState
    {
        Wild,
        Batte
    }

    #endregion

    public sealed class PokemonUnit : UnitBase
    {
        #region Values

        private Pokemon pokemonInformation;

        private PokemonState currentState;

        #endregion

        #region Getters

        public Pokemon GetPokemonInformation() => this.pokemonInformation;

        internal bool IsBattleState => this.currentState.Equals(PokemonState.Batte);

        #endregion

        #region In

        public override void UpdateUnit()
        {
            if (this.currentState.Equals(PokemonState.Wild))
                base.UpdateUnit();
        }

        public override void InteractTrigger()
        {
        }

        public void SetState(PokemonState state)
        {
            if (this.currentState.Equals(state))
                return;

            this.currentState = state;

            Logger.AddLog(this, "Pokemon Unit Switching state to: " + state.ToString());
        }

        public void TriggerStartIntro()
        {
            
        }

        #endregion
    }
}