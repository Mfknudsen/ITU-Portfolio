#region Packages

using System.Linq;
using Runtime.Battle.Actions;
using Runtime.Battle.Systems.Interfaces;
using Runtime.Pokémon;
using UnityEngine;

#endregion

namespace Runtime.Weathers.Climate
{
    public class FogWeather : ClimateWeather, IAccuracyModify, IFinalModifier
    {
        #region Values

        [Header("Move Modification"), SerializeField]
        private Type[] typesUnaffected;

        [Header("Move Modification"), SerializeField]
        private SpecialAddons addonEffected;

        #endregion

        #region Interface Overrides

        //IAccuracyModify
        public bool CanEffect(PokemonMove pokemonMove)
        {
            return pokemonMove.GetAccuracy() != -1 || this.typesUnaffected.Contains(pokemonMove.GetMoveType());
        }

        public float Effect(PokemonMove pokemonMove)
        {
            return 0.8f;
        }

        //IFinalModifier
        public float Modify(PokemonMove pokemonMove)
        {
            return pokemonMove.GetSpecialAddons().Contains(this.addonEffected) ? 1.5f : 1;
        }

        #endregion
    }
}