using Runtime.Battle.Actions;

namespace Runtime.Battle.Systems.Interfaces
{
    public interface IAccuracyModify
    {
        public float Effect(PokemonMove pokemonMove);
    }
}
