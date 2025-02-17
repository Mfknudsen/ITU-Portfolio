#region Packages

using Runtime.Battle.Systems;
using Runtime.Battle.Systems.Spots;
using Runtime.Pokémon;
using System.Collections.Generic;

#endregion

namespace Runtime.AI.Battle.Evaluator.Virtual
{
    public class VirtualSpotOversight
    {
        public readonly List<VirtualSpot> spots;

        public VirtualSpotOversight()
        {
            this.spots = new List<VirtualSpot>();

            foreach (Spot spot in BattleSystem.instance.GetSpotOversight().GetSpots())
            {
                VirtualSpot virtualSpot = new VirtualSpot(spot.GetActivePokemon());
                virtualSpot.SetRelations(
                    spot.GetLeft()?.GetActivePokemon(),
                    spot.GetRight()?.GetActivePokemon(),
                    spot.GetStrafeLeft()?.GetActivePokemon(),
                    spot.GetStrafeRight()?.GetActivePokemon(),
                    spot.GetFront()?.GetActivePokemon()
                );
                this.spots.Add(virtualSpot);
            }
        }

        public VirtualSpot GetPokemonSpot(Pokemon pokemon)
        {
            foreach (VirtualSpot virtualSpot in this.spots)
            {
                VirtualPokemon virtualPokemon = virtualSpot.virtualPokemon;
                if (virtualPokemon.GetActualPokemon() == pokemon || virtualPokemon.GetFakePokemon() == pokemon)
                    return virtualSpot;
            }

            return null;
        }
    }

    public class VirtualSpot
    {
        public readonly VirtualPokemon virtualPokemon;
        public Pokemon left, right, strafeRight, strafeLeft, front;

        public VirtualSpot(Pokemon pokemon) =>
            this.virtualPokemon = new VirtualPokemon(pokemon);

        public VirtualSpot(VirtualPokemon virtualPokemon) =>
            this.virtualPokemon = new VirtualPokemon(
                virtualPokemon.GetFakePokemon(),
                virtualPokemon.GetActualPokemon());

        public void SetRelations(Pokemon l, Pokemon r, Pokemon sR, Pokemon sL, Pokemon f)
        {
            this.left = l;
            this.right = r;
            this.strafeLeft = sL;
            this.strafeRight = sR;
            this.front = f;
        }
    }
}