using Runtime.Pokémon;

namespace Runtime.Battle.Systems.Interfaces
{
    public interface IOnPokemonEnter
    {
        public void Trigger(Pokemon pokemon);
    }
}
