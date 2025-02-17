#region SDK

using Runtime.Battle.Systems.Interfaces;
using UnityEngine;

#endregion

namespace Runtime.Pokémon.Abilities
{
    [CreateAssetMenu(menuName = "Ability/Levitate")]
    public class Levitate : Ability, IImmuneAttackType
    {
        [SerializeField] private TypeName immuneType;
        
        public bool MatchType(TypeName type)
        {
            return type == this.immuneType;
        }

        public override void TriggerEnable(AbilityTrigger trigger, Pokemon currentPokemon)
        {
        }

        public override void TriggerDisable(AbilityTrigger trigger, Pokemon currentPokemon)
        {
        }
    }
}
