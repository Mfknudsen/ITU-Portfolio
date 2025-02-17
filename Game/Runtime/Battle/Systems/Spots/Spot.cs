#region Packages

using System.Collections.Generic;
using Runtime.Pokémon;
using UnityEngine;

#endregion

namespace Runtime.Battle.Systems.Spots
{
    public sealed class Spot : MonoBehaviour
    {
        #region Values

        [SerializeField] private bool needNew = true;
        [SerializeField] private Pokemon activePokemon;
        [SerializeField] private Spot left, right, front, strafeLeft, strafeRight;
        [SerializeField] private Transform currentTransform;
        [SerializeField] private BattleMember battleMember;

        [SerializeField] private int id;

        #endregion

        #region Build In States

        private void OnDrawGizmosSelected()
        {
            Vector3 pos = this.transform.position;
            if (this.left != null)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawLine(pos, this.left.transform.position);
            }

            if (this.right != null)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(pos, this.right.transform.position);
            }

            if (this.strafeLeft != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(pos, this.strafeLeft.transform.position);
            }

            if (this.front != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(pos, this.front.transform.position);
            }

            if (this.strafeRight != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(pos, this.strafeRight.transform.position);
            }
        }

        #endregion

        #region Getters

        public Spot GetLeft() =>
            this.left;

        public Spot GetRight() =>
            this.right;

        public Spot GetFront() =>
            this.front;

        public Spot GetStrafeLeft() =>
            this.strafeLeft;

        public Spot GetStrafeRight() =>
            this.strafeRight;

        public List<Spot> GetAllAdjacentSpots()
        {
            List<Spot> result = new List<Spot>();

            if (this.front != null)
                result.Add(this.front);

            if (this.left != null)
                result.Add(this.left);

            if (this.right != null)
                result.Add(this.right);

            if (this.strafeLeft != null)
                result.Add(this.strafeLeft);

            if (this.strafeRight != null)
                result.Add(this.strafeRight);

            return result;
        }

        public List<Spot> GetAllAdjacentOneSideSpots(bool ally)
        {
            if (!ally)
            {
                return new List<Spot>
                {
                    this.GetFront(),
                    this.GetStrafeLeft(),
                    this.GetStrafeRight()
                };
            }

            return new List<Spot>
            {
                this.GetStrafeLeft(),
                this.GetStrafeRight()
            };
        }

        public bool GetNeedNew() =>
            this.needNew;

        public Pokemon GetActivePokemon() =>
            this.activePokemon;

        public Transform GetTransform() =>
            this.currentTransform;

        public BattleMember GetBattleMember() =>
            this.battleMember;

        public bool GetIsAlly() =>
            this.battleMember.GetTeamAffiliation();

        public int GetID()
        {
            return this.id;
        }

        #endregion

        #region Setters

        public void SetLeft(Spot set) =>
            this.left = set;

        public void SetRight(Spot set) =>
            this.right = set;

        public void SetFront(Spot set) =>
            this.front = set;

        public void SetStrafeLeft(Spot set) =>
            this.strafeLeft = set;

        public void SetStrafeRight(Spot set) =>
            this.strafeRight = set;

        public void SetNeedNew(bool set) =>
            this.needNew = set;

        public void SetActivePokemon(Pokemon set) =>
            this.activePokemon = set;

        public void SetTransform() =>
            this.currentTransform = this.transform;

        public void SetBattleMember(BattleMember member) =>
            this.battleMember = member;

        public void SetID(int set) =>
            this.id = set;

        #endregion

        #region In

        public void SetRelations(int selfIndex, int allyCount, List<Spot> opponentSpots)
        {
            if (opponentSpots.Count == 0)
                return;

            if (opponentSpots.Count == 1)
                this.SetFront(opponentSpots[0]);
            else if (opponentSpots.Count == 2)
            {
                if (selfIndex > 1)
                {
                    this.SetFront(opponentSpots[0]);
                    this.SetStrafeLeft(opponentSpots[1]);
                }
                else
                {
                    this.SetFront(opponentSpots[1]);
                    this.SetStrafeRight(opponentSpots[0]);
                }
            }
            else
            {
                if (allyCount == 3)
                {
                    if (selfIndex == 1)
                    {
                        this.SetFront(opponentSpots[2]);
                        this.SetStrafeRight(opponentSpots[1]);
                    }
                    else if (selfIndex == 2)
                    {
                        this.SetFront(opponentSpots[1]);
                        this.SetStrafeLeft(opponentSpots[2]);
                        this.SetStrafeRight(opponentSpots[0]);
                    }
                    else
                    {
                        this.SetFront(opponentSpots[0]);
                        this.SetStrafeLeft(opponentSpots[1]);
                    }
                }
                else if (allyCount == 2)
                {
                    if (selfIndex == 1)
                    {
                        this.SetFront(opponentSpots[2]);
                        this.SetStrafeRight(opponentSpots[1]);
                    }
                    else
                    {
                        this.SetFront(opponentSpots[0]);
                        this.SetStrafeLeft(opponentSpots[1]);
                    }
                }
                else
                {
                    this.SetFront(opponentSpots[1]);
                    this.SetStrafeLeft(opponentSpots[2]);
                    this.SetStrafeRight(opponentSpots[0]);
                }
            }
        }

        public void DestroySelf() => 
            Destroy(this.gameObject);

        #endregion

        #region Out

        public List<Spot> GetAllOneSide()
        {
            bool continueCheck = true;
            List<Spot> result = new List<Spot> { this };

            while (continueCheck)
            {
                continueCheck = false;

                foreach (Spot s in result)
                {
                    if (!result.Contains(s.GetLeft()))
                    {
                        result.Add(s.GetLeft());
                        continueCheck = true;
                    }

                    if (result.Contains(s.GetRight())) continue;

                    result.Add(s.GetRight());
                    continueCheck = true;
                }
            }

            return result;
        }

        #endregion
    }
}