#region Packages

using System;
using Runtime.Battle.Actions;

#endregion

namespace Runtime.Pokémon
{
    [Serializable]
    public class MoveStats
    {
        public PokemonMove[] learnedMoves = new PokemonMove[4];

        public int[] levelLearnableMoveKeys = Array.Empty<int>();

        public PokemonMove[] levelLearnableMove = Array.Empty<PokemonMove>();

        public PokemonMove[] tmLearnableMove = Array.Empty<PokemonMove>();

        public PokemonMove[] tutorLearnableMove = Array.Empty<PokemonMove>();
    }
}