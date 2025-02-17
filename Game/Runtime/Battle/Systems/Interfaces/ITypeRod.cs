using Runtime.Pokémon;

namespace Runtime.Battle.Systems.Interfaces
{
    public interface ITypeRod
    {
        public bool CanAbsorb(TypeName typeName, Pokemon pokemon);

        public bool ImmuneDamage();
    }
}
