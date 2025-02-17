#region Packages

using System;
using System.Collections;
using Runtime.Items;
using UnityEngine;

#endregion

namespace Runtime.Pokémon
{
    [CreateAssetMenu(fileName = "MegaStone", menuName = "Item/Create new Mega Stone")]
    public class MegaStone : Item, IHoldableItem
    {
        [Header("Mega Stone:")] [SerializeField]
        private Pokemon affectedPokemon;

        #region Getters

        public Pokemon GetAffectedPokemon() => this.affectedPokemon;

        #endregion

        #region Out

        public override bool IsUsableTarget(Pokemon pokemon)
        {
            throw new NotImplementedException();
        }


        public override IEnumerator Operation()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}