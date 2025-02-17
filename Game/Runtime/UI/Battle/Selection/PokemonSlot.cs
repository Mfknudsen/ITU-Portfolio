#region Packages

using Runtime.Pokémon;
using TMPro;
using UnityEngine;

#endregion

namespace Runtime.UI.Battle.Selection
{
    public class PokemonSlot : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI gui;

        private PokemonSelection pokemonSelection;
        private Pokemon pokemon;

        public void SetPokemon(PokemonSelection _pokemonSelection, Pokemon _pokemon)
        {
            this.pokemonSelection = _pokemonSelection;
            this.pokemon = _pokemon;

            this.gui.text = _pokemon is null ? "" : _pokemon.GetName();
        }

        public void Trigger()
        {
            this.pokemonSelection.SendPokemon(this.pokemon);
        }
    }
}