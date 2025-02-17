#region Packages

using System.Collections.Generic;
using Runtime.Pokémon;
using Runtime.Trainer;
using UnityEngine;

#endregion

namespace Runtime.AI
{
    [System.Serializable]
    public class NpcTeam : Team
    {
        [SerializeField] private List<PossiblePokemon> allPossible;

        public void Select(Team toAddTo)
        {
            foreach (PossiblePokemon possiblePokemon in this.allPossible)
                toAddTo.AddNewPokemonToTeam(possiblePokemon.GetRandomFromList());
        }
    }

    [System.Serializable]
    internal struct PossiblePokemon
    {
        [SerializeField] private List<Pokemon> pokemons;

        public Pokemon GetRandomFromList()
        {
            return this.pokemons[Random.Range(0, this.pokemons.Count - 1)];
        }
    }
}