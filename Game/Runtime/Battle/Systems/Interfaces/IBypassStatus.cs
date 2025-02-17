using Runtime.Pokémon;

namespace Runtime.Battle.Systems.Interfaces
{
    public interface IBypassStatus
    {
        public bool CanEffect(Type type);
    }
}
