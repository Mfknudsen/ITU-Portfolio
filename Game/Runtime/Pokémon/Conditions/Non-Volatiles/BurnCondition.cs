#region SDK

using System.Collections;
using UnityEngine;

#endregion

namespace Runtime.Pokémon.Conditions.Non_Volatiles
{
    [CreateAssetMenu(fileName = "Condition", menuName = "Condition/Create new Non-Volatile Condition/Burn", order = 1)]
    public class BurnCondition : NonVolatileCondition
    {
        #region Values
        [SerializeField] private float damage;
        [SerializeField] private float n, increaseN = 1;
        #endregion

        #region Getters

        public override Condition GetCondition()
        {
            Condition result = this;

            if (result.GetIsInstantiated()) return result;
            
            result = Instantiate(this);
            result.SetIsInstantiated(true);

            return result;
        }

        public float GetDamage()
        {
            this.n += this.increaseN;
            return this.damage * this.n;
        }
        #endregion

        #region Setters
        public void SetDamage(int maxHP)
        {
            this.damage = maxHP / 16;
        }
        #endregion

        #region In

        public override void Reset()
        {
            throw new System.NotImplementedException();
        }

        public bool Done()
        {
            throw new System.NotImplementedException();
        }

        public virtual IEnumerator Operation()
        {
            yield return null;
        }
        #endregion
    }
}