#region Packages

using Runtime.Communication;
using UnityEngine;

#endregion

namespace Runtime.Items
{
    public abstract class BattleItem : Item
    {
        [SerializeField] private BattleBagSlot battleBagSlot;
        [SerializeField] protected Chat[] chatOnUse;
        [SerializeField] protected string userName;

        public void SetOnUse(Chat[] chats)
        {
            this.chatOnUse = chats;
        }

        public void SetUserName(string user)
        {
            this.userName = user;
        }

        public BattleBagSlot GetBattleBagSlot()
        {
            return this.battleBagSlot;
        }

        public void SetBattleBagSlot(BattleBagSlot battleBagSlot)
        {
            this.battleBagSlot = battleBagSlot;
        }
    }
}