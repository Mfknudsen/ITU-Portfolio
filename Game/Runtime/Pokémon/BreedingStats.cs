#region Packages

using System;
using Runtime.Battle.Actions;

#endregion

namespace Runtime.Pokémon
{
    [Serializable]
    public class BreedingStats
    {
        public Pokemon[] breedingLearnedMoveKeys;

        public PokemonMove[] breedingLearnedMove;

        public EggGroup eggGroup;

        public int hatchTimeMin, hatchTimeMax;
    }
}