#region Packages

using System.Collections;
using System.Linq;
using Runtime.AI.Battle.Evaluator;
using Runtime.AI.Battle.Evaluator.Virtual;
using Runtime.Battle.Systems;
using Runtime.Battle.Systems.Spots;
using Runtime.Items;
using Runtime.Pokémon;
using Runtime.Systems;
using UnityEngine;

#endregion

namespace Runtime.Battle.Actions
{
    [CreateAssetMenu(fileName = "ItemAction", menuName = "Action/Create new Item Action")]
    public class ItemAction : BattleAction
    {
        #region Values

        [Header("Item Action:")] [SerializeField]
        private Item toUse;

        [SerializeField] private BattleMember battleMember;

        #endregion

        #region Getters

        public bool GetToUse()
        {
            return this.toUse;
        }

        #endregion

        #region Setters

        public void SetToUse(Item set)
        {
            this.toUse = set;
        }

        public void SetBattleMember(BattleMember set)
        {
            this.battleMember = set;
        }

        #endregion

        #region Overrides

        public override float Evaluate(Pokemon user, Pokemon target, VirtualBattle virtualBattle,
            PersonalitySetting personalitySetting)
        {
            Debug.LogError("Evaluate Items");

            return 0;
        }


        public override IEnumerator Operation()
        {
            this.done = false;

            BattleItem battleItem = (BattleItem)this.toUse;

            foreach (Spot spot in BattleSystem.instance.GetSpotOversight().GetSpots()
                         .Where(spot => spot.GetActivePokemon() == this.currentPokemon))
            {
                battleItem.SetUserName(spot.GetBattleMember().GetName());
                break;
            }

            battleItem.SetTarget(this.targets[0].GetActivePokemon());
            battleItem.SetOnUse(this.chatOnActivation);

            OperationsContainer container = new OperationsContainer();
            container.Add(this.toUse);
            this.operationManager.AddAsyncOperationsContainer(container);
            while (!this.toUse.IsOperationDone)
                yield return null;

            this.battleMember.GetInventory().RemoveItem(this.toUse);

            this.done = true;
        }

        #endregion
    }
}