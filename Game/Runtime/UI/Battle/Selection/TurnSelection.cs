#region Packages

using System.Linq;
using Runtime.Battle.Actions;
using Runtime.Battle.Systems;
using Runtime.Battle.Systems.Spots;
using Runtime.Pokémon;
using UnityEngine;

#endregion

// ReSharper disable ParameterHidesMember
namespace Runtime.UI.Battle.Selection
{
    public class TurnSelection : MonoBehaviour
    {
        #region Values

        [SerializeField] private SelectionMenu selectionMenu;
        [SerializeField] private GameObject background;
        [SerializeField] private TurnMoveSlot[] moveSlots;

        private SpotOversight spotOversight;
        private Pokemon pokemon;

        #endregion

        #region In

        public void Setup()
        {
            this.spotOversight = BattleSystem.instance.GetSpotOversight();
        }

        public void DisplaySelection(Pokemon pokemon)
        {
            this.pokemon = pokemon;

            this.background.SetActive(true);

            for (int i = 0; i < this.moveSlots.Length; i++)
            {
                PokemonMove move = pokemon.GetMoveByIndex(i);

                this.moveSlots[i].SetMove(move);
            }
        }

        public void DisableDisplaySelection()
        {
            this.background.SetActive(false);
        }

        public void ReceiveAction(BattleAction battleAction)
        {
            battleAction.SetCurrentPokemon(this.pokemon);

            if (this.spotOversight.GetToDefaultTargeting())
            {
                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (Spot spot in this.spotOversight.GetSpots())
                {
                    bool enemy = spot.GetActivePokemon() != battleAction.GetCurrentPokemon();

                    if (!battleAction.GetDefaultTargetEnemy() && enemy ||
                        battleAction.GetDefaultTargetEnemy() && !enemy) continue;

                    battleAction.SetTargets(spot.GetActivePokemon());

                    this.pokemon.SetBattleAction(battleAction);

                    break;
                }
            }
            else
                this.selectionMenu.DisplaySelection(SelectorGoal.Target, battleAction);
        }

        #region Side Bottuns

        public void SwitchButton()
        {
            SwitchAction action = BattleSystem.instance.InstantiateSwitchAction();

            action.SetCurrentPokemon(this.pokemon);

            this.selectionMenu.DisplaySelection(SelectorGoal.Switch, action);
        }

        public void ItemButton()
        {
            BattleSystem battleSystem = BattleSystem.instance;
            ItemAction action = battleSystem.InstantiateItemAction();
            foreach (Spot spot in battleSystem.GetSpotOversight().GetSpots()
                .Where(spot => spot.GetActivePokemon() == this.pokemon))
            {
                action.SetBattleMember(spot.GetBattleMember());
                break;
            }

            action.SetCurrentPokemon(this.pokemon);

            this.selectionMenu.DisplaySelection(SelectorGoal.Item, action);
        }

        public void RunButton()
        {
            Debug.Log("Run Not Implemented");
        }

        #endregion

        #endregion
    }
}