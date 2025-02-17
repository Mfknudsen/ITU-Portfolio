#region Packages

using System;
using System.Linq;
using Runtime.Systems;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.AI.Battle.BeforeEffects
{
    [Serializable]
    public class BeforeEffectContainer
    {
        #region Values

        [SerializeField, Required] private OperationManager operationManager; 
        [SerializeField] private NpcBattleBeforeEffect[] effects;

        #endregion

        #region In

        public void OnValidate()
        {
            for (int i = 0; i < this.effects.Length; i++)
            {
                NpcBattleBeforeEffect effect = this.effects[i];
                this.effects[i] = effect.GetIsInstantiated() ? effect : effect.GetInstantiatedEffect();
            }
        }

        public void SetInput<T>(object i) where T : NpcBattleBeforeEffect
        {
            foreach (NpcBattleBeforeEffect effect in this.effects.Where(e => e is T))
                effect.SetInput(i);
        }

        public void Trigger()
        {
            this.operationManager.AddOperationsContainer(new OperationsContainer(this.effects));
        }

        #endregion

        #region Out

        public bool AllDone()
        {
            return !this.effects.Any(e => !e.IsOperationDone);
        }

        #endregion
    }
}