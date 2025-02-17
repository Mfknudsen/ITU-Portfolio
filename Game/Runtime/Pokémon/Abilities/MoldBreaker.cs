using Runtime.Battle.Systems.Interfaces;
using UnityEngine;

namespace Runtime.Pokémon.Abilities
{
    [CreateAssetMenu(menuName = "Ability/Mold Breaker", fileName = "Mold Breaker")]
    public class MoldBreaker : Ability, IIgnoreAbilities
    {
        public override void TriggerEnable(AbilityTrigger trigger, Pokemon currentPokemon)
        {
        }

        public override void TriggerDisable(AbilityTrigger trigger, Pokemon currentPokemon)
        {
        }
    }
}
