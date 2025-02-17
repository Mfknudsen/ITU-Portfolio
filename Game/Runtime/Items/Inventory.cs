#region Packages

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

namespace Runtime.Items
{
    public class Inventory : MonoBehaviour
    {
        #region Values

        [Header("Object Reference:")] [SerializeField]
        private readonly List<ItemContainer> items = new List<ItemContainer>();

        [Header("Bag Categories:")] [SerializeField]
        private List<ItemType> medicine;

        [SerializeField] private List<ItemType> balls, battle, berries, other, tm, treasure, key;

        #endregion

        #region Getters

        public ItemContainer[] GetAllItems()
        {
            return this.items.ToArray();
        }

        public Item GetItemByIndex(int i)
        {
            return this.items[i].item;
        }

        public bool IsItemInBag(Item i)
        {
            return this.items.Any(container => container.item.GetItemName() == i.GetItemName());
        }

        #region - Bag Categories

        public ItemContainer[] GetMedicine()
        {
            return this.items.Where(i => this.medicine.Contains(i.item.GetItemType())).ToArray();
        }

        public ItemContainer[] GetPokeballs()
        {
            return this.items.Where(i => this.balls.Contains(i.item.GetItemType())).ToArray();
        }

        public ItemContainer[] GetBattleItems()
        {
            return this.items.Where(i => this.battle.Contains(i.item.GetItemType())).ToArray();
        }

        public ItemContainer[] GetBerries()
        {
            return this.items.Where(i => this.berries.Contains(i.item.GetItemType())).ToArray();
        }

        public ItemContainer[] GetOther()
        {
            return this.items.Where(i => this.other.Contains(i.item.GetItemType())).ToArray();
        }

        public ItemContainer[] GetTM()
        {
            return this.items.Where(i => this.tm.Contains(i.item.GetItemType())).ToArray();
        }

        public ItemContainer[] GetTreasure()
        {
            return this.items.Where(i => this.treasure.Contains(i.item.GetItemType())).ToArray();
        }

        public ItemContainer[] GetKeyItems()
        {
            return this.items.Where(i => this.key.Contains(i.item.GetItemType())).ToArray();
        }

        #endregion

        #endregion

        #region Setters

        #endregion

        #region In

        public void AddItem(Item i)
        {
            bool foundExisting = false;
            foreach (ItemContainer container in this.items.Where(
                container => container.item.GetItemName() == i.GetItemName()))
            {
                foundExisting = true;
                container.count++;
                break;
            }

            if (foundExisting) return;

            ItemContainer itemContainer = new ItemContainer(i);
            this.items.Add(itemContainer);
        }

        public void RemoveItem(Item i)
        {
            foreach (ItemContainer container in this.items.Where(
                container => container.item.GetItemName() == i.GetItemName()))
            {
                container.count--;
                break;
            }
        }

        #endregion
    }

    public class ItemContainer
    {
        public ItemContainer(Item item)
        {
            this.item = item;
            this.count = 1;
        }
        
        public Item item;
        public int count;
    }
}