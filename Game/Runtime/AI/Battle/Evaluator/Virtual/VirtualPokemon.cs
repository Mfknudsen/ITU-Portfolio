#region Packages

using Runtime.Pokémon;
using UnityEngine;
using Type = System.Type;

#endregion

namespace Runtime.AI.Battle.Evaluator.Virtual
{
    public class VirtualPokemon
    {
        private readonly Pokemon pokemon;
        private readonly Pokemon fakePokemon;
        private bool isKnown;
        public bool isAlly;

        public VirtualPokemon(Pokemon pokemon)
        {
            this.pokemon = pokemon;
            this.fakePokemon = Object.Instantiate(pokemon);
            SetNonSerializedValues(this.fakePokemon, pokemon);
            this.fakePokemon.ResetForAIMemory();
            this.isKnown = false;
        }

        public VirtualPokemon(Pokemon fake, Pokemon original)
        {
            this.pokemon = original;
            this.fakePokemon = Object.Instantiate(fake);
            SetNonSerializedValues(this.fakePokemon, fake);
            this.fakePokemon.ResetForAIMemory();
            this.isKnown = false;
        }

        #region Getters

        public Pokemon GetFakePokemon() => this.fakePokemon;

        public Pokemon GetActualPokemon() => this.pokemon;

        public bool GetKnown() => 
            this.isKnown;

        #endregion

        #region In

        public void OnEnterCheck(Pokemon toCheck)
        {
            if (this.pokemon != toCheck)
                return;

            this.isKnown = true;
        }

        public void OnAbilityTrigger(Pokemon toCheck, Ability ability)
        {
            if (this.pokemon != toCheck)
                return;

            try
            {
                Ability[] abilities = toCheck.GetAbilities();
                Type type = ability.GetType();
                if (abilities[0].GetType() == type)
                    this.fakePokemon.SetFirstAbility(ability);
                else if (abilities[1].GetType() == type)
                    this.fakePokemon.SetSecondAbility(ability);
                else if (abilities[2].GetType() == type) this.fakePokemon.SetHiddenAbility(ability);
            }
            catch
            {
                //Ignore
            }
        }

        #endregion

        #region Internal

        private static void SetNonSerializedValues(Pokemon fake, Pokemon original)
        {
            fake.VirtualSetConditionOversight(original.GetConditionOversight());
        }

        #endregion
    }
}