#region Packages

using System.Linq;
using Runtime.Battle.Systems;
using Runtime.Battle.Systems.Interfaces;
using Runtime.Pokémon;
using UnityEngine;

#endregion

namespace Runtime.Weathers.Irritants
{
    public class PollenStormWeather : IrritantWeather, IStatModifier
    {
        #region Values

        [SerializeField] private TypeName[] immuneTypes;
        [SerializeField] private WeatherTerrain weatherTerrain;

        #endregion

        public override void Setup()
        {
            base.Setup();


            if (this.amplified)
                BattleSystem.instance.GetWeatherManager().ApplyTerrain(this.weatherTerrain);
        }

        #region Interface Overrides

        public float Modify(Pokemon pokemon, Stat stat)
        {
            return pokemon.GetTypes().Any(type => this.immuneTypes.Contains(type.GetTypeName()) && stat == Stat.SpAtk)
                ? 0.75f
                : 1;
        }

        #endregion
    }
}