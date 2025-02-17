using Runtime.Battle.Actions;

namespace Runtime.Battle.Systems.Interfaces
{
    public interface IPriorityModify
    {
        public float Effect(PokemonMove pokemonMove);
    }
}
