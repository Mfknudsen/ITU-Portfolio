#region Libraries

using System.Collections.Generic;
using Runtime.AI;
using Runtime.AI.World;
using Runtime.Core;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.World.Overworld.Spawner
{
    public sealed class HerdSpawner : PokemonSpawner
    {
        #region Values

        private UnitHerd spawnedHerd;

        [SerializeField, Required] private PokemonHerdSpawnList spawnList;

        #endregion

        #region Internal

        protected override void CheckState()
        {
            if (this.spawnedHerd == null && Random.Range(0, 100) < this.spawnList.SpawnChance)
                this.SpawnHerd();

            if (this.checkStateTimer == null)
                this.checkStateTimer = new Timer(30, this.CheckState);
            else
                this.checkStateTimer.Reset();
        }

        private void SpawnHerd()
        {
            PokemonUnit spawnedHerdLeader = this.spawnList.HerdLeader.InstantiateUnitPrefab(PokemonState.Wild,
                Vector3.zero, Quaternion.identity, active: false);

            List<PokemonUnit> spawnedGuardians = new List<PokemonUnit>();
            for (int i = 0; i < Random.Range(this.spawnList.GuardianMin, this.spawnList.GuardianMax); i++)
            {
                PokemonUnit unit = this.spawnList.HerdGuardians.RandomFrom()
                    .InstantiateUnitPrefab(PokemonState.Wild, Vector3.zero, Quaternion.identity, active: false);
                spawnedGuardians.Add(unit);
            }

            List<PokemonUnit> spawnedOthers = new List<PokemonUnit>();
            for (int i = 0; i < Random.Range(this.spawnList.OtherMin, this.spawnList.OtherMax); i++)
            {
                PokemonUnit unit = this.spawnList.HerdOthers.RandomFrom()
                    .InstantiateUnitPrefab(PokemonState.Wild, Vector3.zero, Quaternion.identity, active: false);
                spawnedOthers.Add(unit);
            }

            this.spawnedHerd = new GameObject().AddComponent<UnitHerd>()
                .Setup(spawnedHerdLeader, spawnedGuardians, spawnedOthers);
        }

#if UNITY_EDITOR
        protected override string SpawnListName() =>
            this.spawnList != null ? this.spawnList.name : "";
#endif

        #endregion
    }
}