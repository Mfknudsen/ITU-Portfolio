#region Packages

using System;
using Runtime.Items;
using UnityEngine;

#endregion

namespace Runtime.Pokémon.Evolutions
{
    [Serializable]
    public class ItemEvolution : EvolutionMethod
    {
        #region Values

        [SerializeField] private Item itemToUse;

        [SerializeField] private Pokemon evolveTo;

        #endregion

        #region Out

        public override bool Check(object[] inputObjects = null)
        {
            return false;
        }

        #endregion
    }
}
