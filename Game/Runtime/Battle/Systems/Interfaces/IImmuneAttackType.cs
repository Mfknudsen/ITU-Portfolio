using Runtime.Pokémon;

namespace Runtime.Battle.Systems.Interfaces
{
    // ReSharper disable once IdentifierTypo
    public interface IImmuneAttackType
    {
        public bool MatchType(TypeName type);
    }
}
