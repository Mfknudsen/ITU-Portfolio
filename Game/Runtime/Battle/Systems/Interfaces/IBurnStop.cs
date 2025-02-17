using Runtime.Pokémon;

namespace Runtime.Battle.Systems.Interfaces
{
    public interface IBurnStop
    {
        public bool CanStopBurn(Pokemon pokemon);
    }
}
