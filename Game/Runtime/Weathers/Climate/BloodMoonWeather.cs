#region Packages

using Runtime.Battle.Actions;
using Runtime.Battle.Systems.Interfaces;
using Runtime.Pokémon;
using UnityEngine;

#endregion

namespace Runtime.Weathers.Climate
{
    public class BloodMoonWeather : ClimateWeather, IOnSuperEffective, IBypassStatus, IPriorityModify
    {
        #region Values

        [Header("Bypass Status"), SerializeField]
        private Type statusType;

        [Header("Priority Modify"), SerializeField]
        private Type priorityType;

        #endregion

        #region Interface Overrides

        //IOnSuperEffective
        float IOnSuperEffective.Effect(PokemonMove pokemonMove)
        {
            return 1.2f;
        }

        //IBypassStatus
        public bool CanEffect(Type type)
        {
            if (!this.amplified) return false;

            return type == this.statusType;
        }

        //IPriorityModify
        float IPriorityModify.Effect(PokemonMove pokemonMove)
        {
            bool isStatus = pokemonMove.GetCategory() == Category.Status;

            if (!isStatus)
                return 0;

            if (this.amplified)
                return 1;

            return this.priorityType == pokemonMove.GetMoveType()
                ? 1
                : 0;
        }

        #endregion
    }
}