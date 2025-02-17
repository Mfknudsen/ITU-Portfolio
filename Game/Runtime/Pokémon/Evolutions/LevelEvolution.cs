#region Packages

using System;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.Pokémon.Evolutions
{
    [Serializable]
    public class LevelEvolution : EvolutionMethod
    {
        #region Values

        [SerializeField] private int level;

        [SerializeField, Required] private Pokemon evolveTo;

        #endregion

        #region Out

        public override bool Check(object[] inputObjects = null)
        {
            return false;
        }

        #endregion
    }
}
