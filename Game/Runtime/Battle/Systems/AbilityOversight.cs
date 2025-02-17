#region Packages

using System.Collections.Generic;
using Runtime.Pokémon;

#endregion

namespace Runtime.Battle.Systems
{
    public class AbilityOversight
    {
        private List<Ability> abilities;

        #region Getters

        // ReSharper disable once ReturnTypeCanBeEnumerable.Global
        public List<Ability> GetAbilities()
        {
            return this.abilities;
        }

        #endregion

        #region In

        public void Setup()
        {
            this.abilities = new List<Ability>();
        }

        public void AddAbility(Ability ability)
        {
            if (!this.abilities.Contains(ability) && !(ability is null)) this.abilities.Add(ability);
        }

        public void RemoveAbility(Ability ability)
        {
            if (this.abilities.Contains(ability)) this.abilities.Remove(ability);
        }

        #endregion

        #region Out

        public List<T> ListOfSpecific<T>()
        {
            List<T> result = new List<T>();

            foreach (Ability ability in this.abilities)
            {
                if (ability is T tAbility)
                    result.Add(tAbility);
            }

            return result;
        }

        #endregion
    }
}