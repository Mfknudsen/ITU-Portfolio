#region Libraries

using Runtime.Battle.Systems;
using Runtime.Battle.Systems.Interfaces;
using Runtime.Battle.Systems.Spots;
using Runtime.Pokémon;
using UnityEngine;

#endregion

namespace Runtime.Weathers.Energy
{
    [CreateAssetMenu(fileName = "Battle Aura", menuName = "Pokemon/Weather/Energy/Battle Aura")]
    public class BattleAuraWeather : EnergyWeather, IBypassImmune, IOnPokemonEnter
    {
        #region Values

        [SerializeField] private Type type;

        #endregion

        public override void Setup()
        {
            base.Setup();

            foreach (Spot spot in BattleSystem.instance.GetSpotOversight().GetSpots())
                IncreaseCritical(spot.GetActivePokemon());
        }

        #region Internal

        private static void IncreaseCritical(Pokemon pokemon)
        {
        }

        #endregion

        #region Interface Overrides

        //IBypassImmune
        public bool CanEffect(TypeName attackType, TypeName defendsType)
        {
            if (!this.amplified)
                return false;

            return (attackType == TypeName.Fighting || attackType == TypeName.Normal) &&
                   defendsType == TypeName.Ghost;
        }

        //IOnPokemonEnter
        public void Trigger(Pokemon pokemon)
        {
            IncreaseCritical(pokemon);
        }

        #endregion
    }
}