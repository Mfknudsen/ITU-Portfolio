using Runtime.Pokémon;

namespace Runtime.Battle.Systems.Interfaces
{
    public interface IBypassImmune
    {
        public bool CanEffect(TypeName attackType, TypeName defendsType);
    }
}
