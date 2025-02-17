#region Packages

using System.Collections;
using Runtime.Pokémon;
using Runtime.Systems;
using UnityEngine;

#endregion

namespace Runtime.Battle.Systems.Static_Operations
{
    public class DamagePokemon : IOperation
    {
        private bool done;
        private readonly float damage, totalTime;
        private const float SplitTime = 200;
        private readonly Pokemon target;

        public DamagePokemon(Pokemon target, float damage, float totalTime)
        {
            this.damage = damage;
            this.totalTime = totalTime;
            this.target = target;
        }

        public bool IsOperationDone => this.done;

        public IEnumerator Operation()
        {
            float damageApplied = 0, damageOverTime = this.damage / SplitTime;

            while (damageApplied < this.damage)
            {
                if (damageApplied + damageOverTime >= this.damage)
                    damageOverTime = this.damage - damageApplied;
                
                damageApplied += damageOverTime;

                this.target.ReceiveDamage(damageOverTime);

                yield return new WaitForSeconds(this.totalTime / SplitTime);
            }

            this.done = true;
        }

        public void OperationEnd()
        {
        }
    }

    public struct DamageProfile
    {
        
    }
}
