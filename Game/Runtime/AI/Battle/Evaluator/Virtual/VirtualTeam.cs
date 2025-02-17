#region Packages

using System.Collections.Generic;
using System.Linq;
using Runtime.Pokémon;
using Runtime.Trainer;

#endregion

namespace Runtime.AI.Battle.Evaluator.Virtual
{
    public class VirtualTeam 
    {
        private readonly Team team;

        // ReSharper disable once CollectionNeverUpdated.Local
        // ReSharper disable once IdentifierTypo
        private readonly List<VirtualPokemon> rememberPokemons;

        public VirtualTeam(Team team)
        {
            this.team = team;

            this.rememberPokemons = new List<VirtualPokemon>();

            for (int i = 0; i < 6; i++)
            {
                Pokemon pokemon = this.team.GetPokemonByIndex(i);

                if (pokemon == null)
                    continue;

                this.rememberPokemons.Add(
                    new VirtualPokemon(pokemon)
                );
            }
        }
        
        public Pokemon[] GetRememberedPokemons()
        {
            return this.rememberPokemons.Select(rememberPokemon => rememberPokemon.GetFakePokemon()).ToArray();
        }
    }
}
