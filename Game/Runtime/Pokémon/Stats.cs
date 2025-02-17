#region Packages

using System;
using UnityEngine;

#endregion

namespace Runtime.Pokémon
{
    [Serializable]
    public struct Stats
    {
        [SerializeField]
        private int hp,
            attack,
            defence,
            spAtk,
            spDef,
            speed,
            accuracy,
            evasion,
            critical;

        public int this[Stat stat]
        {
            get
            {
                return stat switch
                {
                    Stat.HP => this.hp,
                    Stat.Attack => this.attack,
                    Stat.Defence => this.defence,
                    Stat.SpAtk => this.spAtk,
                    Stat.SpDef => this.spDef,
                    Stat.Speed => this.speed,
                    Stat.Accuracy => this.accuracy,
                    Stat.Evasion => this.evasion,
                    Stat.Critical => this.critical,
                    _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, null)
                };
            }
        }
    }
}