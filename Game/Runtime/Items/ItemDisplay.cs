#region Packages

using Runtime.Battle.Actions;
using Runtime.UI.Battle.Selection;
using TMPro;
using UnityEngine;

#endregion

// ReSharper disable ParameterHidesMember
namespace Runtime.Items
{
    public class ItemDisplay : MonoBehaviour
    {
        #region Values

        [SerializeField] private TextMeshProUGUI nameText, countText;
        private ItemSelection itemSelection;
        private ItemContainer itemContainer;
        private ItemAction itemAction;

        #endregion

        #region Set

        public void Setup(ItemSelection itemSelection, ItemContainer itemContainer, ItemAction itemAction)
        {
            this.itemSelection = itemSelection;
            this.itemContainer = itemContainer;
            this.itemAction = itemAction;

            this.nameText.text = itemContainer.item.GetItemName();
            this.countText.text = "" + itemContainer.count;
        }

        #endregion

        #region In

        public void Trigger()
        {
            this.itemAction.SetToUse(this.itemContainer.item);
            this.itemSelection.ReceiveAction(this.itemAction);
        }

        #endregion
    }
}