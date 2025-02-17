using Runtime.Battle.Systems.Interfaces;
using UnityEngine;

namespace Runtime.Pokémon.Abilities
{
    [CreateAssetMenu(menuName = "Ability/Lighting Rod", fileName = "Lighting Rod")]
    public class LightningRod : Ability, ITypeRod
    {
        [SerializeField] private TypeName typeName;
        
        public override void TriggerEnable(AbilityTrigger trigger, Pokemon currentPokemon)
        {
        }

        public override void TriggerDisable(AbilityTrigger trigger, Pokemon currentPokemon)
        {
        }

        public bool CanAbsorb(TypeName attackTypeName, Pokemon pokemon)
        {
            return attackTypeName == this.typeName;
        }

        public bool ImmuneDamage()
        {
            return true;
        }
    }
}