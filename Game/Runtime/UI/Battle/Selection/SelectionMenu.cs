#region Packages

using Runtime.Battle.Actions;
using Runtime.Pokémon;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.UI.Battle.Selection
{
    #region Enums

    public enum SelectorGoal
    {
        Switch,
        Item,
        Target,
        Turn
    }

    #endregion

    public class SelectionMenu : MonoBehaviour
    {
        #region Values

        [SerializeField, Required] private PokemonSelection pokemonSelection;
        [SerializeField, Required] private ItemSelection itemSelection;
        [SerializeField, Required] private TargetSelection targetSelection;
        [SerializeField, Required] private TurnSelection turnSelection;

        #endregion

        #region In

        public void Setup()
        {
            this.pokemonSelection.Setup();
            this.itemSelection.Setup();
            this.targetSelection.Setup();
            this.turnSelection.Setup();
        }

        public void DisplaySelection(SelectorGoal goal, object parse)
        {
            this.DisableDisplaySelection();

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (goal)
            {
                case SelectorGoal.Switch:
                    this.pokemonSelection.DisplaySelection((SwitchAction)parse);
                    break;

                case SelectorGoal.Item:
                    this.itemSelection.DisplaySelection((ItemAction)parse);
                    break;

                case SelectorGoal.Target:
                    this.targetSelection.DisplaySelection((BattleAction)parse);
                    break;

                case SelectorGoal.Turn:
                    this.turnSelection.DisplaySelection((Pokemon)parse);
                    break;
            }
        }

        public void DisableDisplaySelection()
        {
            this.pokemonSelection.DisableDisplaySelection();
            this.itemSelection.DisableDisplaySelection();
            this.targetSelection.DisableDisplaySelection();
            this.turnSelection.DisableDisplaySelection();
        }

        #endregion
    }
}