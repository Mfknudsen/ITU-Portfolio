#region Packages

using System.Collections;
using System.Linq;
using Runtime.Battle.Systems;
using Runtime.Battle.Systems.Interfaces;
using Runtime.Battle.Systems.Spots;
using Runtime.Pokémon;
using UnityEngine;

#endregion

namespace Runtime.Weathers.Irritants
{
    [CreateAssetMenu(fileName = "Sandstorm", menuName = "Pokemon/Weather/Irritants/Sandstorm")]
    public class SandstormWeather : IrritantWeather, IOnTurnEnd, IStatModifier
    {
        [Header("On Turn End"), SerializeField]
        private TypeName ground, rock, steel;

        [Header("On Pokemon Enter"), SerializeField]
        private Type boostType;

        [SerializeField] private Stat boostStat;

        #region Interface Overrides

        private bool IsImmune(Pokemon pokemon)
        {
            return pokemon.GetTypes().Any(type => type.GetTypeName() == this.ground ||
                                                  type.GetTypeName() == this.steel ||
                                                  type.GetTypeName() == this.rock);
        }

        public IEnumerator Operation()
        {
            SpotOversight oversight = BattleSystem.instance.GetSpotOversight();
            float secPerPokeMove = 200 * BattleSystem.instance.GetSecPerPokeMove();

            foreach (Spot spot in oversight.GetSpots())
            {
                Pokemon pokemon = spot.GetActivePokemon();

                if (pokemon == null || this.IsImmune(pokemon))
                    continue;

                int partsDealt = this.amplified ? 8 : 16;

                int damagePerTarget = pokemon.GetCalculatedStat(Stat.HP) / partsDealt;
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

        public void Trigger(Pokemon pokemon)
        {
            if (pokemon.GetTypes().Contains(this.boostType))
                pokemon.EffectMultiplierStage(1, this.boostStat);
        }

        public float Modify(Pokemon pokemon, Stat stat)
        {
            if (pokemon.GetTypes().Any(type => type.GetTypeName() == this.rock &&
                                               stat == Stat.SpDef))
                return 1.5f;

            return !this.amplified
                ? 1
                : pokemon.GetTypes().Any(type => type.GetTypeName() == this.steel ||
                                                 type.GetTypeName() == this.ground &&
                                                 stat == Stat.Defence)
                    ? 1.5f
                    : 1;
        }

        #endregion
    }
}