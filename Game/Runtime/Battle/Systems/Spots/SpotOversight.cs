#region SDK

using System;
using System.Collections.Generic;
using Runtime.Trainer;
using UnityEngine;

#endregion

namespace Runtime.Battle.Systems.Spots
{
    public class SpotOversight
    {
        #region Values

        private readonly List<Spot> list;
        private int counts;
        private bool defaultTargets;

        #endregion

        #region Getters

        public SpotOversight()
        {
            this.list = new List<Spot>();
            this.counts = 0;
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Global
        public List<Spot> GetSpots()
        {
            return this.list;
        }

        public bool GetToDefaultTargeting()
        {
            return this.defaultTargets;
        }

        #endregion

        #region In

        public void SetupSpots(BattleMember[] members, Transform[] initializedSpots, BattleSystem battleSystem)
        {
            if (members.Length != initializedSpots.Length)
                throw new Exception("Total count of battle members is not equal to initialized spot length");

            for (int index = 0; index < members.Length; index++)
            {
                Transform t = initializedSpots[index];
                BattleMember battleMember = members[index];
                Team team = battleMember.GetTeam();
                for (int i = 0; i < battleMember.GetSpotsToOwn(); i++)
                {
                    if (!team.CanSendMorePokemon())
                    {
                        battleMember.ForceHasAllSpots();
                        break;
                    }

                    Spot spot = battleSystem.CreateSpot();
                    Transform spotTransform = spot.transform;
                    spotTransform.position = t.position;
                    spotTransform.rotation = t.rotation;
                    spot.SetBattleMember(battleMember);
                    this.SetSpot(spot);
                    battleMember.SetOwnedSpot(spot);
                }
            }
        }

        public void Reorganise(bool removeEmpty)
        {
            List<Spot> enemies = new List<Spot>(),
                allies = new List<Spot>(),
                toRemove = new List<Spot>();

            foreach (Spot spot in this.list)
            {
                if (spot.GetActivePokemon() == null && removeEmpty)
                    toRemove.Add(spot);
                else
                {
                    if (spot.GetIsAlly())
                        allies.Add(spot);
                    else
                        enemies.Add(spot);
                }
            }

            if (removeEmpty)
            {
                foreach (Spot spot in toRemove)
                {
                    this.list.Remove(spot);

                    if (allies.Contains(spot))
                        allies.Remove(spot);
                    else if (enemies.Contains(spot))
                        enemies.Remove(spot);

                    spot.DestroySelf();
                }
            }

            #region Set Relations

            for (int i = 0; i < allies.Count; i++)
            {
                if (i > 0)
                    allies[i].SetLeft(allies[i - 1]);
                if (i < allies.Count - 1)
                    allies[i].SetRight(allies[i + 1]);

                allies[i].SetRelations(i + 1, allies.Count, enemies);
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].SetRelations(i + 1, enemies.Count, allies);
            }

            #endregion

            this.defaultTargets = this.list.Count == 2;
        }

        #endregion

        #region Internal

        private void SetSpot(Spot spot)
        {
            if (spot == null)
                return;

            spot.SetID(this.counts);
            this.counts += 1;

            this.list.Add(spot);
        }

        #endregion
    }
}