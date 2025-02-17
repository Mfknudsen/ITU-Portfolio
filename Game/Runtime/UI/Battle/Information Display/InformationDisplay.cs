#region Packages

using System.Collections.Generic;
using System.Linq;
using Runtime.Battle.Systems.Spots;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.UI.Battle.Information_Display
{
    public class InformationDisplay : MonoBehaviour
    {
        #region Values

        [SerializeField, Required] private Transform allyOut, enemyOut;
        [SerializeField] private List<PokemonDisplaySlot> allyDisplays, enemyDisplays;

        private readonly Vector3[] allyPositions = new Vector3[3];

        private bool ready;

        #endregion

        #region Build In States

        private void Start()
        {
            int i = 2;
            foreach (PokemonDisplaySlot pokemonDisplay in this.allyDisplays)
            {
                this.allyPositions[i] = pokemonDisplay.transform.position;
                i--;

                pokemonDisplay.Setup();
                pokemonDisplay.transform.position = this.allyOut.position;
            }

            foreach (PokemonDisplaySlot pokemonDisplay in this.enemyDisplays)
            {
                pokemonDisplay.Setup();
                pokemonDisplay.transform.position = this.enemyOut.position;
            }
            
            this.ready = true;
        }

        private void Update()
        {
            if (!this.ready) return;

            Animate(this.allyDisplays, this.allyOut.position);
            Animate(this.enemyDisplays, this.enemyOut.position);
        }

        #endregion

        #region In

        public void UpdateSlots(SpotOversight spotOversight)
        {
            int allyIndex = 0, enemyIndex = 0, allyOffset = 0;
            allyOffset += spotOversight.GetSpots()
                .Where(spot => spot.GetIsAlly())
                .Count(spot => spot.GetActivePokemon() != null);

            //From Count to Index Value
            allyOffset--;

            foreach (Spot spot in spotOversight.GetSpots())
            {
                if (spot.GetIsAlly())
                {
                    PokemonDisplaySlot slot = this.allyDisplays[allyIndex];

                    slot.SetPokemon(spot.GetActivePokemon());

                    if (allyOffset - allyIndex >= 0 && allyOffset - allyIndex < this.allyPositions.Length)
                        slot.SetOriginPosition(this.allyPositions[allyOffset - allyIndex]);

                    allyIndex++;
                }
                else
                {
                    PokemonDisplaySlot slot = this.enemyDisplays[enemyIndex];

                    slot.SetPokemon(spot.GetActivePokemon());

                    enemyIndex++;
                }
            }
        }

        #endregion

        #region Internal

        private static void Animate(List<PokemonDisplaySlot> list, Vector3 outPos)
        {
            foreach (PokemonDisplaySlot pokemonDisplay in list)
            {
                if (!pokemonDisplay.GetActive())
                {
                    pokemonDisplay.transform.position =
                        Vector3.Lerp(pokemonDisplay.transform.position, outPos, 1.5f * Time.deltaTime);
                }
                else
                {
                    pokemonDisplay.transform.position = Vector3.Lerp(pokemonDisplay.transform.position,
                        pokemonDisplay.GetOriginPosition(), 1.5f * Time.deltaTime);

                    if (Vector3.Distance(pokemonDisplay.transform.position, pokemonDisplay.GetOriginPosition()) < 0.1f)
                        pokemonDisplay.transform.position = pokemonDisplay.GetOriginPosition();
                }
            }
        }

        #endregion
    }
}