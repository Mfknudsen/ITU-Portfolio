#region Packages

using System.Collections;
using System.Linq;
using Runtime.Battle.Actions;
using Runtime.Battle.Systems;
using Runtime.Battle.Systems.Interfaces;
using Runtime.Battle.Systems.Spots;
using Runtime.Battle.Systems.Static_Operations;
using Runtime.Pokémon;
using Runtime.Systems;
using Runtime.Weathers._Extra;
using Runtime.Weathers.Climate;
using UnityEngine;

#endregion

namespace Runtime.Weathers.Energy
{
    public class ThunderStormWeather : EnergyWeather, IOnTurnEnd, IAccuracyModify, IOperation
    {
        #region Values

        [SerializeField] private TypeName[] immuneTypes;
        [SerializeField] private WeatherTerrain elWeatherTerrain;
        [SerializeField] private GameObject lightingPrefab;
        private bool done;

        public override void Setup()
        {
            base.Setup();

            if (this.amplified)
                BattleSystem.instance.GetWeatherManager().ApplyTerrain(this.elWeatherTerrain);
        }

        #endregion

        public bool IsOperationDone => this.done;

        public IEnumerator Operation()
        {
            this.done = false;
            WeatherManager weatherManager = BattleSystem.instance.GetWeatherManager();
            bool raining = weatherManager.GetAll()[0] is RainWeather;

            foreach (Spot spot in BattleSystem.instance.GetSpotOversight().GetSpots())
            {
                if (Random.Range(0, 10) > 1)
                    continue;

                Pokemon pokemon = spot.GetActivePokemon();

                if (!raining && pokemon.GetTypes().Any(type => this.immuneTypes.Contains(type.GetTypeName())))
                    continue;

                //Damage
                OperationsContainer container = new OperationsContainer();
                int damagePerTarget = pokemon.GetCalculatedStat(Stat.HP) / 10;
                DamagePokemon damagePokemon = new DamagePokemon(pokemon, damagePerTarget, 1);
                container.Add(damagePokemon);

                //Visual
                GameObject obj = Instantiate(this.lightingPrefab);
                ThunderStormLighting lighting = obj.GetComponent<ThunderStormLighting>();
                container.Add(lighting);

                this.operationManager.AddOperationsContainer(container);
            }

            this.done = true;

            yield break;
        }

        public void OperationEnd()
        {
        }

        public float Effect(PokemonMove pokemonMove)
        {
            if (pokemonMove.GetMoveType().GetTypeName() != TypeName.Electric)
                return 1;

            pokemonMove.SetAccuracy(100);

            return 1;
        }
    }
}