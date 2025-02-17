using Runtime.Pokémon;

namespace Runtime.Battle.Systems.Interfaces
{
    // ReSharper disable once IdentifierTypo
    public interface ICritModifier
    {
        public float Modify(Pokemon pokemon);
    }
}
