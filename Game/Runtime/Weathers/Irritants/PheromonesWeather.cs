#region Packages

using System.Linq;
using Runtime.Battle.Systems;
using Runtime.Battle.Systems.Interfaces;
using Runtime.Pokémon;
using Runtime.Pokémon.Conditions;
using UnityEngine;

#endregion

namespace Runtime.Weathers.Irritants
{
    public class PheromonesWeather : IrritantWeather, IStatModifier, IOnPokemonEnter
    {
        #region Values

        [Header("Stat Modifier"), SerializeField]
        private TypeName[] affectedTypes;

        [SerializeField] private Stat[] affectedStats;

        [Header("On Pokemon Enter"), SerializeField]
        private VolatileCondition confusion;

        #endregion

        public override void Setup()
        {
            base.Setup();

            foreach (Pokemon pokemon in BattleSystem.instance.GetSpotOversight().GetSpots()
                .Select(spot => spot.GetActivePokemon()).Where(pokemon =>
                    pokemon.GetTypes().Any(type => this.affectedTypes.Contains(type.GetTypeName()))))
            {
                pokemon.GetConditionOversight().ApplyVolatileCondition(this.confusion);
            }
        }

        #region Interface Overrides

        //IStatModifier
        public float Modify(Pokemon pokemon, Stat stat)
        {
            return this.affectedStats.Contains(stat) &&
                   pokemon.GetTypes().Any(type => this.affectedTypes.Contains(type.GetTypeName()))
                ? 1.25f
                : 1;
        }

        //IOnPokemonEnter
        public void Trigger(Pokemon pokemon)
        {
        }

        #endregion
    }
}