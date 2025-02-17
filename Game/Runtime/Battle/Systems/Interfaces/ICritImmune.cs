using Runtime.Pokémon;

namespace Runtime.Battle.Systems.Interfaces
{
    // ReSharper disable once IdentifierTypo
    public interface ICritImmune
    {
        public bool CanEffect(Pokemon pokemon);
    }
}
