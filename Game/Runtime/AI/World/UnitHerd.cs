#region Libraries

using System.Collections.Generic;
using NodeCanvas.Framework;
using UnityEngine;

#endregion

namespace Runtime.AI.World
{
    public sealed class UnitHerd : MonoBehaviour
    {
        #region Values

        private PokemonUnit leader;
        private List<PokemonUnit> guardians;
        private List<PokemonUnit> others;

        #endregion

        #region In

        public UnitHerd Setup(PokemonUnit leader, List<PokemonUnit> guardians, List<PokemonUnit> others)
        {
            this.leader = leader;
            this.guardians = guardians;
            this.others = others;

            Blackboard blackboard = leader.GetBlackboard();
            blackboard.AddVariable("HerdGuardians", guardians);
            blackboard.AddVariable("HerdOthers", guardians);
            leader.TriggerStartIntro();
            leader.AddDisableEventListener(this.OnLeaderDisabled);

            foreach (PokemonUnit unit in guardians)
            {
                blackboard = unit.GetBlackboard();
                blackboard.AddVariable("HerdLeader", leader);
                blackboard.AddVariable("HerdGuardians", guardians);
                blackboard.AddVariable("HerdOthers", guardians);
                unit.TriggerStartIntro();
                unit.AddDisableEventListener(this.OnGuardianDisabled);
            }

            foreach (PokemonUnit unit in others)
            {
                blackboard = unit.GetBlackboard();
                blackboard.AddVariable("HerdLeader", leader);
                blackboard.AddVariable("HerdGuardians", guardians);
                blackboard.AddVariable("HerdOthers", guardians);
                unit.TriggerStartIntro();
                unit.AddDisableEventListener(this.OnOtherDisabled);
            }

            return this;
        }

        #endregion

        #region Internal

        private void OnLeaderDisabled(UnitBase unit)
        {
            this.leader = null;
        }

        private void OnGuardianDisabled(UnitBase unit)
        {
            this.guardians.Remove(unit as PokemonUnit);
        }

        private void OnOtherDisabled(UnitBase unit)
        {
            this.others.Remove(unit as PokemonUnit);
        }

        #endregion
    }
}