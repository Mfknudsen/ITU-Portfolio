#region SDK

using System.Collections.Generic;
using Runtime.Battle.Actions;
using Runtime.Battle.Systems;
using Runtime.Battle.Systems.Spots;
using Runtime.Player;
using Runtime.Pokémon;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

#endregion

// ReSharper disable PossibleNullReferenceException
// ReSharper disable ParameterHidesMember
namespace Runtime.UI.Battle.Selection
{
    public class TargetSelection : MonoBehaviour
    {
        #region Values

        [SerializeField, Required] private PlayerManager playerManager;
        [SerializeField] private GameObject background;
        [SerializeField] private TargetSlot[] enemies, allies;
        private SpotOversight oversight;
        private bool isAlly;
        private BattleAction action;

        #endregion

        #region In

        public void Setup()
        {
            this.oversight = BattleSystem.instance.GetSpotOversight();
            this.isAlly = this.playerManager.GetBattleMember().GetTeamAffiliation();
        }

        public void DisplaySelection(BattleAction action)
        {
            this.action = action;

            this.background.SetActive(true);

            Spot currentSpot = null;

            List<Spot> eSpots = new List<Spot>(), aSpots = new List<Spot>();

            foreach (Spot spot in this.oversight.GetSpots())
            {
                if (spot.GetActivePokemon() == action.GetCurrentPokemon())
                    currentSpot = spot;

                if (spot.GetIsAlly() == this.isAlly)
                    aSpots.Add(spot);
                else
                    eSpots.Add(spot);
            }

            for (int i = 0; i < this.enemies.Length; i++)
            {
                if (i < eSpots.Count)
                    this.enemies[i].SetPokemon(this, eSpots[i]);
                else
                    this.enemies[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < this.allies.Length; i++)
            {
                if (i < aSpots.Count)
                    this.allies[i].SetPokemon(this, aSpots[i]);
                else
                    this.allies[i].gameObject.SetActive(false);
            }

            if (action is PokemonMove pokemonMove)
            {
                // ReSharper disable once IdentifierTypo
                bool[] targetable = pokemonMove.GetTargetable();
                bool selfTarget = targetable[5], allyClose = targetable[3], allyLong = targetable[4];
                bool enemyFront = targetable[0], enemyStrafe = targetable[1], enemyLong = targetable[2];

                foreach (TargetSlot targetSlot in this.enemies)
                {
                    Button button = targetSlot.gameObject.GetComponent<Button>();
                    Spot targetSpot = targetSlot.GetSpot();

                    if (targetSpot == currentSpot.GetFront())
                        button.enabled = enemyFront;
                    else if (targetSpot == currentSpot.GetStrafeLeft() || targetSpot == currentSpot.GetStrafeRight())
                        button.enabled = enemyStrafe;
                    else
                        button.enabled = enemyLong;
                }

                foreach (TargetSlot targetSlot in this.allies)
                {
                    Button button = targetSlot.gameObject.GetComponent<Button>();
                    Spot targetSpot = targetSlot.GetSpot();

                    if (targetSpot == currentSpot)
                        button.enabled = selfTarget;
                    else if (targetSpot == currentSpot.GetLeft() || targetSpot == currentSpot.GetRight())
                        button.enabled = allyClose;
                    else
                        button.enabled = allyLong;
                }
            }
        }

        public void DisableDisplaySelection()
        {
            this.background.SetActive(false);
        }

        public void ReceiveSpot(Spot spot)
        {
            Pokemon pokemon = this.action.GetCurrentPokemon();

            if (pokemon is null) return;

            this.action.SetTargets(spot.GetActivePokemon());

            pokemon.SetBattleAction(this.action);
        }

        #endregion
    }
}