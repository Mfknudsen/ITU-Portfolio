#region Packages

using UnityEngine;

#endregion

namespace Runtime.Pokémon
{
    [CreateAssetMenu(fileName = "Ability", menuName = "Pokemon/Create New Ability", order = 2)]
    public abstract class Ability : ScriptableObject
    {
        #region Values

        [SerializeField] private int index;
        [SerializeField] private string abilityName;
        [SerializeField] protected AbilityTrigger enableTrigger, disableTrigger;
        [SerializeField, TextArea] private string description;

        protected Pokemon affectedPokemon;

        [SerializeField] private bool active;

        #endregion

        #region Getters

        public bool GetActive()
        {
            return this.active;
        }

        #endregion

        #region Setters

        public void SetAffectedPokemon(Pokemon pokemon)
        {
            this.affectedPokemon = pokemon;
        }

        public void SetActive(bool set)
        {
            this.active = set;
        }

        #endregion

        #region In

        public abstract void TriggerEnable(AbilityTrigger trigger, Pokemon currentPokemon);

        public abstract void TriggerDisable(AbilityTrigger trigger, Pokemon currentPokemon);

        #endregion
    }
}