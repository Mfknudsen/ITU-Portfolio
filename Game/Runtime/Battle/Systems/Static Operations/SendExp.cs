using System.Collections;
using Runtime.Pokémon;
using Runtime.Systems;
using UnityEngine;

namespace Runtime.Battle.Systems.Static_Operations
{
    public class SendExp : IOperation
    {
        private bool done;
        private readonly float totalTime;
        private readonly int points;
        private readonly Pokemon pokemon;

        public SendExp(Pokemon pokemon, int points, float totalTime)
        {
            this.pokemon = pokemon;
            this.points = points;
            this.totalTime = totalTime;
        }

        public bool IsOperationDone => this.done;

        public IEnumerator Operation()
        {
            this.done = false;

            // ReSharper disable once InconsistentNaming
            float splitEXP = this.points / 200, applied = 0;

            while (applied < this.points)
            {
                if (applied + splitEXP > this.points)
                    splitEXP = this.points - applied;

                this.pokemon.ReceiveExp((int)splitEXP);
                applied += splitEXP;

                yield return new WaitForSeconds(this.totalTime / 200);
            }

            this.done = true;
        }

        public void OperationEnd()
        {
        }
    }
}