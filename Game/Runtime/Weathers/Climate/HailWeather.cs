#region Packages

using System.Collections;
using System.Linq;
using Runtime.Battle.Systems;
using Runtime.Battle.Systems.Interfaces;
using Runtime.Battle.Systems.Spots;
using Runtime.Pokémon;
using UnityEngine;

#endregion

namespace Runtime.Weathers.Climate
{
    [CreateAssetMenu(fileName = "Hail", menuName = "Pokemon/Weather/Irritants/Hail")]
    public class HailWeather : ClimateWeather, IOnTurnEnd
    {
        [SerializeField] private Type[] immuneTypes;

        #region Interface Overrides
        
        private bool IsImmune(Pokemon pokemon)
        {
            return pokemon.GetTypes().Any(type => this.immuneTypes.Contains(type));
        }

        #endregion

        public IEnumerator Operation()
        {
            SpotOversight oversight = BattleSystem.instance.GetSpotOversight();
            float secPerPokeMove = 200 * BattleSystem.instance.GetSecPerPokeMove();

            foreach (Spot spot in oversight.GetSpots())
            {
                Pokemon pokemon = spot.GetActivePokemon();

                if (pokemon == null || this.IsImmune(pokemon))
                    continue;

                int damagePerTarget = pokemon.GetCalculatedStat(Stat.HP) / 16;
                float damageApplied = 0, damageOverTime = damagePerTarget / secPerPokeMove;


                while (damageApplied < damagePerTarget)
                {
                    if (damageApplied + damageOverTime >= damagePerTarget)
                        damageOverTime = damagePerTarget - damageApplied;

                    damageApplied += damageOverTime;

                    pokemon.ReceiveDamage(damageOverTime);

                    yield return new WaitForSeconds(BattleSystem.instance.GetSecPerPokeMove() / secPerPokeMove);
                }
            }
        }
    }
}