#region Packages

using System.Collections;
using Runtime.Communication;
using Runtime.Systems;
using UnityEngine;

#endregion

namespace Runtime.Pokémon.Conditions.Non_Volatiles
{
    [CreateAssetMenu(fileName = "Condition", menuName = "Condition/Create new Non-Volatile Condition/Paralysis", order = 1)]
    public class ParalysisCondition : NonVolatileCondition, IOperation
    {
        [SerializeField] private Chat onEffectChat;
        
        public override void Reset()
        {
            throw new System.NotImplementedException();
        }

        public bool IsOperationDone => throw new System.NotImplementedException();

        public IEnumerator Operation()
        {
            throw new System.NotImplementedException();
        }

        public void OperationEnd()
        {
        }
    }
}