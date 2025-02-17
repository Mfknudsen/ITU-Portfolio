#region Packages

using System.Linq;
using Runtime.Battle.Systems.Interfaces;
using Runtime.Pokémon;
using UnityEngine;

#endregion

namespace Runtime.Weathers.Irritants
{
    public class DustStormWeather : IrritantWeather, IStatModifier
    {
        #region Values

        [SerializeField] private TypeName boostType, reduceType;
        [SerializeField] private Stat boostStat, reduceStat;
        [SerializeField] private float boostValue, reduceValue;

        #endregion

        #region Interface Overrides

        public float Modify(Pokemon pokemon, Stat stat)
        {
            if (pokemon.GetTypes().Any(type => type.GetTypeName() == this.boostType) && stat == this.boostStat)
                return this.boostValue;

            if (pokemon.GetTypes().Any(type => type.GetTypeName() == this.reduceType) && stat == this.reduceStat)
                return this.reduceValue;

            return 1;
        }

        #endregion
    }
}