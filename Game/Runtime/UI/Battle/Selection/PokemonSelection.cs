#region Packages

using System.Linq;
using Runtime.Battle.Actions;
using Runtime.Battle.Systems;
using Runtime.Player;
using Runtime.Pokémon;
using Runtime.Trainer;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.UI.Battle.Selection
{
    public class PokemonSelection : MonoBehaviour
    {
        #region Values

        [SerializeField, Required] private PlayerManager playerManager;
        [SerializeField, Required] private GameObject background;
        [SerializeField, Required] private PokemonSlot[] pokemonSlots;

        private Team playerTeam;
        private SwitchAction switchAction;

        #endregion

        #region In

        public void Setup() =>
            this.playerTeam = this.playerManager.GetTeam();

        public void DisplaySelection(SwitchAction action)
        {
            this.switchAction = action;

            this.background.SetActive(true);

            for (int i = 0; i < 6; i++)
                this.pokemonSlots[i].SetPokemon(
                    this, this.playerTeam.GetPokemonByIndex(i));
        }

        public void DisableDisplaySelection() =>
            this.background.SetActive(false);

        public void SendPokemon(Pokemon pokemon)
        {
            if (pokemon == null) return;

            this.switchAction.SetNextPokemon(
                pokemon);

            this.switchAction.SetSpot(
                BattleSystem.instance.GetSpotOversight().GetSpots()
                    .FirstOrDefault(s => s.GetActivePokemon() == this.switchAction.GetCurrentPokemon()));

            this.switchAction.SetTeam(this.playerManager.GetTeam());

            pokemon.SetGettingSwitched(
                true);

            this.switchAction.GetCurrentPokemon().SetBattleAction(this.switchAction);

            this.switchAction = null;
        }

        #endregion
    }
}