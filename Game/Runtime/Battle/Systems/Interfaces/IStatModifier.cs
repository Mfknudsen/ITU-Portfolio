using Runtime.Pokémon;

namespace Runtime.Battle.Systems.Interfaces
{
    public interface IStatModifier
    {
        public float Modify(Pokemon pokemon, Stat stat);
    }
}