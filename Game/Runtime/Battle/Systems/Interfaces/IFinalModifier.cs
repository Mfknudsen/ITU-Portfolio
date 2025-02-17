using Runtime.Battle.Actions;

namespace Runtime.Battle.Systems.Interfaces
{
    public interface IFinalModifier
    {
        public float Modify(PokemonMove pokemonMove);
    }
}
