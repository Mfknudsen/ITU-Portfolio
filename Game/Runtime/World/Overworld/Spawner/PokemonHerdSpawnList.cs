#region Libraries

using System.Collections.Generic;
using Runtime.Pokémon;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.World.Overworld.Spawner
{
    public sealed class PokemonHerdSpawnList : ScriptableObject
    {
        #region Values

        [SerializeField, MinValue(0), MaxValue(100)]
        private int spawnChance;

        [SerializeField, Required] private Pokemon herdLeader;
        [SerializeField] private List<Pokemon> herdGuardians, herdOthers;

        [SerializeField] private int guardianMin, guardianMax, otherMin, otherMax;

        #endregion

        #region Getters

        public Pokemon HerdLeader => this.herdLeader;

        public List<Pokemon> HerdGuardians => this.herdGuardians;

        public List<Pokemon> HerdOthers => this.herdOthers;

        public int GuardianMin => this.guardianMin;

        public int GuardianMax => this.guardianMax;

        public int OtherMin => this.otherMin;

        public int OtherMax => this.otherMax;

        public int SpawnChance => this.spawnChance;

        #endregion
    }
}