#region SDK

using UnityEngine;

#endregion

namespace Runtime.Pokémon
{
    [CreateAssetMenu(menuName = "Pokemon/Nature")]
    public class Nature : ScriptableObject
    {
        [SerializeField] private string natureName;

        // ReSharper disable once IdentifierTypo
        [SerializeField] private Stat positiveStat, negativeStat;
        [SerializeField] private int positiveBonus, negativeReduction;

        public float GetNatureStat(Stat stat)
        {
            if (stat == this.positiveStat)
                return this.positiveBonus;

            return stat == this.negativeStat ? this.negativeReduction : 1;
        }
    }
}