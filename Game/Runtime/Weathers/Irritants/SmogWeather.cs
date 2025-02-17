#region Packages

using System.Linq;
using Runtime.Battle.Systems;
using Runtime.Battle.Systems.Interfaces;
using Runtime.Pokémon;
using Runtime.Pokémon.Conditions.Non_Volatiles;
using UnityEngine;

#endregion

// ReSharper disable SimplifyLinqExpressionUseAll
namespace Runtime.Weathers.Irritants
{
    public class SmogWeather : IrritantWeather, IOnPokemonEnter
    {
        [SerializeField] private PoisonCondition poison;

        public override void Setup()
        {
            base.Setup();

            foreach (Pokemon pokemon in BattleSystem.instance.GetSpotOversight().GetSpots()
                .Select(spot => spot.GetActivePokemon()).Where(pokemon => this.amplified || !pokemon.GetTypes().Any(type => type.GetTypeName() == TypeName.Poison)))
            {
                pokemon.GetConditionOversight().TryApplyNonVolatileCondition(this.poison);
            }
        }

        public void Trigger(Pokemon pokemon)
        {
            if (this.amplified || !pokemon.GetTypes().Any(type => type.GetTypeName() == TypeName.Poison))
                pokemon.GetConditionOversight().TryApplyNonVolatileCondition(this.poison);
        }
    }
}