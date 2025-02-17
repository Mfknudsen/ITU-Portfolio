#region Packages

using System.Collections.Generic;
using System.Linq;
using Runtime.Battle.Systems;
using Runtime.Weathers;
using UnityEngine;

#endregion

namespace Runtime.AI.Battle.Evaluator.Virtual
{
    public class VirtualBattle
    {
        public readonly VirtualSpotOversight spotOversight;
        public readonly List<Weather> weathers;

        public VirtualBattle()
        {
            this.spotOversight = new VirtualSpotOversight();
            BattleSystem battleSystem = BattleSystem.instance;

            this.weathers = new List<Weather>();
            this.weathers.AddRange(battleSystem.GetWeatherManager().GetAll()
                .Where(o => o != null)
                .Select(Object.Instantiate));
        }

        public VirtualBattle(VirtualBattle oldBattle)
        {
            this.spotOversight = new VirtualSpotOversight();

            foreach (VirtualSpot spot in oldBattle.spotOversight.spots) this.spotOversight.spots.Add(new VirtualSpot(spot.virtualPokemon));

            this.weathers = new List<Weather>();
            foreach (Weather w in oldBattle.weathers.Select(Object.Instantiate))
            {
                w.TickTurn();

                if (w.EffectDone())
                    continue;

                this.weathers.Add(w);
            }
        }
    }
}