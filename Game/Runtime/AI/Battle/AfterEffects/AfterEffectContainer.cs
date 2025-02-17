#region Packages

using System;
using System.Linq;
using Runtime.Systems;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.AI.Battle.AfterEffects
{
    [Serializable]
    public class AfterEffectContainer
    {
        [SerializeField, Required] private OperationManager operationManager;
        [SerializeField] private NpcBattleAfterEffect[] effects;

        #region In

        public void OnValidate()
        {
            for (int i = 0; i < this.effects.Length; i++)
            {
                NpcBattleAfterEffect effect = this.effects[i];
                this.effects[i] = effect.GetIsInstantiated() ? effect : effect.GetInstantiatedEffect();
            }
        }

        public void SetInput<T>(object i) where T : NpcBattleAfterEffect
        {
            foreach (NpcBattleAfterEffect effect in this.effects.Where(e => e is T))
                effect.SetInput(i);
        }

        public void Trigger()
        {
            // ReSharper disable once CoVariantArrayConversion
            this.operationManager.AddOperationsContainer(new OperationsContainer(this.effects));
        }

        #endregion

        #region Out

        public bool AllDone()
        {
            return this.effects.Any(e => !e.IsOperationDone);
        }

        #endregion
    }
}