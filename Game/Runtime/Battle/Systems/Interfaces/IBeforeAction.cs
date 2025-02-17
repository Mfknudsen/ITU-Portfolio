using Runtime.Pokémon;

namespace Runtime.Battle.Systems.Interfaces
{
    public interface IBeforeAction
    {
        public void Modify(Pokemon pokemon);
    }
}
