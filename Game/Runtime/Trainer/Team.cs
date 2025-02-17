#region Packages

using System.Collections.Generic;
using System.Linq;
using Runtime.Pokémon;
using Runtime.Pokémon.Conditions.Non_Volatiles;
using UnityEngine;

#endregion

namespace Runtime.Trainer
{
    public class Team : MonoBehaviour
    {
        #region Values

        [SerializeField] private List<Pokemon> pokemons = new List<Pokemon>(6);
        private readonly List<BoxContainer> boxContainers = new List<BoxContainer>();

        private void OnValidate()
        {
            if (this.pokemons.Count == 6) return;

            List<Pokemon> tempList = new List<Pokemon>();

            foreach (Pokemon p in this.pokemons)
            {
                if (p != null && tempList.Count != 6)
                    tempList.Add(p);
                else if (tempList.Count == 6)
                    break;
            }

            for (int i = 0; i < tempList.Count; i++) this.pokemons[i] = tempList[i];
        }

        #endregion

        #region Getters

        public int GetTeamCount() =>
            this.pokemons.Count(p => p != null);

        public bool HasMorePokemon() =>
            this.pokemons.FirstOrDefault(p => !(p.GetConditionOversight().GetNonVolatileStatus() is FaintedCondition));

        public bool CanSendMorePokemon()
        {
            foreach (Pokemon pokemon in this.pokemons)
            {
                if (pokemon is null) continue;

                if (pokemon.GetInBattle() || pokemon.GetGettingSwitched()) continue;

                if (pokemon.GetConditionOversight().GetNonVolatileStatus() is null)
                    return true;

                if (!(pokemon.GetConditionOversight().GetNonVolatileStatus() is FaintedCondition))
                    return true;
            }

            return false;
        }

        public bool PartOfTeam(Pokemon pokemon)
        {
            foreach (Pokemon p in this.pokemons)
            {
                if (p == pokemon)
                    return true;
            }

            return false;
        }

        public Pokemon GetFirstOut()
        {
            foreach (Pokemon pokemon in this.pokemons)
            {
                if (pokemon is null) continue;

                if (!pokemon.GetInBattle() && !pokemon.GetGettingSwitched())
                    return pokemon;
            }

            return null;
        }

        #endregion

        #region In

        public void Setup()
        {
            for (int i = 0; i < this.pokemons.Count; i++)
            {
                Pokemon pokemon = this.pokemons[i];
                if (pokemon == null || pokemon.GetIsInstantiated()) continue;

                pokemon = Instantiate(pokemon);

                pokemon.SetIsInstantiated(true);

                this.pokemons[i] = pokemon;
            }
        }

        public Pokemon GetPokemonByIndex(int index)
        {
            if (index is < 0 or > 5 || index >= this.pokemons.Count) return null;

            return this.pokemons[index];
        }

        public void SwitchTeamPlaces(Pokemon from, Pokemon to)
        {
            int fromIndex = this.pokemons.IndexOf(from), toIndex = this.pokemons.IndexOf(to);

            (this.pokemons[toIndex], this.pokemons[fromIndex]) = (this.pokemons[fromIndex], this.pokemons[toIndex]);
        }

        public void AddNewPokemonToTeam(Pokemon toAdd)
        {
            for (int i = 0; i < this.pokemons.Capacity; i++)
            {
                if (this.pokemons[i] != null) continue;

                this.pokemons[i] = toAdd;
                return;
            }

            if (this.boxContainers.Any(container => container.AddPokemon(toAdd)))
                return;

            BoxContainer c = new BoxContainer();
            c.AddPokemon(toAdd);
            this.boxContainers.Add(c);
        }

        public void RemovePokemonFromTeam(Pokemon toRemove)
        {
            for (int i = 0; i < this.pokemons.Capacity; i++)
            {
                if (this.pokemons[i] != toRemove) continue;

                this.pokemons[i] = null;
                return;
            }

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            this.boxContainers.Any(boxContainer => boxContainer.RemovePokemon(toRemove));
        }

        #endregion
    }

    internal class BoxContainer
    {
        private readonly List<Pokemon> pokemons = new List<Pokemon>(48);

        public bool AddPokemon(Pokemon toAdd)
        {
            for (int i = 0; i < this.pokemons.Capacity; i++)
            {
                if (this.pokemons[i] != null) continue;

                this.pokemons[i] = toAdd;
                return true;
            }

            return false;
        }

        public bool RemovePokemon(Pokemon toRemove)
        {
            for (int i = 0; i < this.pokemons.Capacity; i++)
            {
                if (this.pokemons[i] != toRemove) continue;

                this.pokemons[i] = toRemove;
                return true;
            }

            return false;
        }
    }
}