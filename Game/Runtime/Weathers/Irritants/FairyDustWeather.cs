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
    public class FairyDustWeather : IrritantWeather, IStatModifier, IOnTurnEnd
    {
        #region Values

        [SerializeField] private WeatherTerrain misty;

        [Header("Stat Modifier")] [SerializeField]
        private TypeName[] noEffectTypes;

        #endregion

        public override void Setup()
        {
            base.Setup();

            if (this.amplified)
                BattleSystem.instance.GetWeatherManager().ApplyTerrain(this.misty);
        }

        #region Interface Overrides

        //IStatModifier
        public float Modify(Pokemon pokemon, Stat stat)
        {
            return pokemon.GetTypes().Any(type => type.GetTypeName() != TypeName.Fairy) && stat == Stat.Evasion
                ? 0.8f
                : 1;
        }

        //IOnTurnEnd
        public IEnumerator Operation()
        {
            float secPerPokeMove = 200 * BattleSystem.instance.GetSecPerPokeMove();

            foreach (Spot spot in BattleSystem.instance.GetSpotOversight().GetSpots())
            {
                Pokemon pokemon = spot.GetActivePokemon();

                if (pokemon.GetTypes().Any(type => this.noEffectTypes.Contains(type.GetTypeName())))
                    continue;

                int damagePerTarget = pokemon.GetCalculatedStat(Stat.HP) / 16;
                float damageApplied = 0, damageOverTime = damagePerTarget / secPerPokeMove;


                while (damageApplied < damagePerTarget)
                {
                    if (damageApplied + damageOverTime >= damagePerTarget)
                        damageOverTime = damagePerTarget - damageApplied;

                    damageApplied += damageOverTime;

                    pokemon.ReceiveDamage(-damageOverTime);

                    yield return new WaitForSeconds(BattleSystem.instance.GetSecPerPokeMove() / secPerPokeMove);
                }
            }
        }

        #endregion
    }
}