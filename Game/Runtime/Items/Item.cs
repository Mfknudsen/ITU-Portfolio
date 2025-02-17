#region Packages

using System.Collections;
using Runtime.Communication;
using Runtime.Player;
using Runtime.Pokémon;
using Runtime.Systems;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.Items
{
    #region Enums

    public enum ItemType
    {
        Berry,
        MegaStone,
        Revive,
        Potion,
    }

    public enum BattleBagSlot
    {
        Battle,
        Pokeball,
        Berries,
        Medicine
    }

    #endregion

    public abstract class Item : ScriptableObject, IOperation
    {
        #region Values

        [SerializeField, Required] protected PlayerManager playerManager;
        [SerializeField, Required] protected OperationManager operationManager;
        [SerializeField, Required] protected ChatManager chatManager;
        
        [Header("Object Reference:")] [SerializeField]
        protected bool isInstantiated;

        [Header("Item Information:")] [SerializeField]
        protected ItemType type = 0;

        [SerializeField] protected string itemName = "";
        [SerializeField, TextArea] protected string description = "";

        [Header("Operation:")] [SerializeField]
        protected Pokemon target;

        [SerializeField] protected bool inUse;
        [SerializeField] protected bool done;

        #region Visual

        [Header("Visual")] [SerializeField, AssetsOnly, AssetSelector(Paths = "Assets/Prefabs/Items", Filter = "t:Prefab")] private GameObject visualPrefab;
        private GameObject instantiateObject;

        #endregion

        #endregion

        #region Getters

        public Item GetItem()
        {
            Item result = this;

            if (result.GetIsInstantiated()) return result;

            result = Instantiate(result);
            result.SetIsInstantiated(true);

            return result;
        }

        public ItemType GetItemType() => this.type;

        public bool GetIsInstantiated() => this.isInstantiated;

        public string GetItemName() => this.itemName;

        public string GetDescription()
        {
            return this.description;
        }

        public bool GetInUse()
        {
            return this.inUse;
        }

        public bool GetDone()
        {
            return this.done;
        }

        public abstract bool IsUsableTarget(Pokemon pokemon);

        public GameObject GetInstantiatedVisualObject()
        {
            return this.instantiateObject;
        }

        public GameObject GetVisualPrefab() => this.visualPrefab;

        #endregion

        #region Setters

        public void SetIsInstantiated(bool set)
        {
            this.isInstantiated = set;
        }

        public void SetInUse(bool set)
        {
            this.inUse = set;
        }

        public virtual void SetTarget(Pokemon set)
        {
            this.target = set;
        }

        #endregion

        #region In

        public void SpawnVisualObject()
        {
            this.instantiateObject = Instantiate(this.visualPrefab);
        }

        #endregion

        #region IOperation

        public bool IsOperationDone => this.done;

        public abstract IEnumerator Operation();

        public virtual void OperationEnd()
        {
        }

        #endregion
    }
}