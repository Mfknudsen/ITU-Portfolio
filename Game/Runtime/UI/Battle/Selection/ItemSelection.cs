#region Packages

using System.Collections.Generic;
using System.Linq;
using Runtime.Battle.Actions;
using Runtime.Battle.Systems;
using Runtime.Battle.Systems.Spots;
using Runtime.Items;
using Runtime.Player;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable ConvertIfStatementToSwitchExpression
// ReSharper disable ParameterHidesMember
namespace Runtime.UI.Battle.Selection
{
    public class ItemSelection : MonoBehaviour
    {
        #region Values

        [SerializeField, Required] private PlayerManager playerManager;

        [SerializeField] private SelectionMenu selectionMenu;
        [SerializeField] private GameObject background;

        [SerializeField] private GameObject itemDisplay, bagSlotSelection;

        [SerializeField] private GameObject itemUIPrefab;
        [SerializeField] private Transform parentTransform;
        [SerializeField] private List<ItemDisplay> displays = new List<ItemDisplay>();

        private readonly List<ItemContainer> potionList = new List<ItemContainer>(),
            pokeballList = new List<ItemContainer>(),
            otherList = new List<ItemContainer>(),
            medicineList = new List<ItemContainer>();

        private SpotOversight spotOversight;
        private ItemAction itemAction;
        private Inventory playerInventory;

        #endregion

        #region In

        public void Setup()
        {
            this.playerInventory = this.playerManager.GetBattleMember().GetInventory();
            this.spotOversight = BattleSystem.instance.GetSpotOversight();
        }

        public void DisplaySelection(ItemAction itemAction)
        {
            this.background.SetActive(true);

            this.itemAction = itemAction;

            foreach (ItemContainer itemContainer in this.playerInventory.GetAllItems()
                         .Where(container => container.item is BattleItem))
            {
                BattleBagSlot slot = ((BattleItem)itemContainer.item).GetBattleBagSlot();

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (slot == BattleBagSlot.Battle && !this.potionList.Contains(itemContainer))
                    this.potionList.Add(itemContainer);
                else if (slot == BattleBagSlot.Pokeball && !this.pokeballList.Contains(itemContainer))
                    this.pokeballList.Add(itemContainer);
                else if (slot == BattleBagSlot.Berries && !this.otherList.Contains(itemContainer))
                    this.otherList.Add(itemContainer);
                else if (slot == BattleBagSlot.Medicine && !this.medicineList.Contains(itemContainer)) this.medicineList.Add(itemContainer);
            }

            this.itemDisplay.SetActive(false);
            this.bagSlotSelection.SetActive(true);
        }

        public void DisplayByBagSlot(int i)
        {
            BattleBagSlot battleBagSlot = (BattleBagSlot)i;

            this.bagSlotSelection.SetActive(false);
            this.itemDisplay.SetActive(true);

            List<ItemContainer> toDisplay;

            if (battleBagSlot == BattleBagSlot.Battle)
                toDisplay = this.potionList;
            else if (battleBagSlot == BattleBagSlot.Pokeball)
                toDisplay = this.pokeballList;
            else if (battleBagSlot == BattleBagSlot.Berries)
                toDisplay = this.otherList;
            else
                toDisplay = this.medicineList;

            foreach (ItemContainer itemContainer in toDisplay)
            {
                ItemDisplay display = Instantiate(this.itemUIPrefab, this.parentTransform).GetComponent<ItemDisplay>();

                display.Setup(this, itemContainer, this.itemAction);

                this.displays.Add(display);
            }
        }

        public void DisableDisplaySelection()
        {
            foreach (ItemDisplay display in this.displays)
                Destroy(display.gameObject);

            this.displays.Clear();

            this.background.SetActive(false);
        }

        public void ReceiveAction(BattleAction battleAction)
        {
            if (this.spotOversight.GetToDefaultTargeting())
            {
                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (Spot spot in this.spotOversight.GetSpots())
                {
                    bool enemy = spot.GetActivePokemon() != battleAction.GetCurrentPokemon();

                    if (!battleAction.GetDefaultTargetEnemy() && enemy ||
                        battleAction.GetDefaultTargetEnemy() && !enemy) continue;

                    battleAction.SetTargets(spot.GetActivePokemon());

                    battleAction.GetCurrentPokemon().SetBattleAction(battleAction);

                    break;
                }
            }
            else
                this.selectionMenu.DisplaySelection(SelectorGoal.Target, battleAction);
        }

        #endregion
    }
}