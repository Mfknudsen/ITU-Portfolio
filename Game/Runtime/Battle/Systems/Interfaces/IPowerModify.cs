using Runtime.Pokémon;

namespace Runtime.Battle.Systems.Interfaces
{
    public interface IPowerModify
    {
        public bool CanModify(Pokemon pokemon);

        public float Modification();
    }
}
