using Runtime.Battle.Actions;

namespace Runtime.Battle.Systems.Interfaces
{
    public interface IMoveModification
    {
        public void Modify(PokemonMove pokemonMove);
    }
}
