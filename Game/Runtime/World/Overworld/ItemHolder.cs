#region Packages

using Runtime.Items;
using UnityEngine;

#endregion

namespace Runtime.World.Overworld
{
    [RequireComponent(typeof(Rigidbody))]
    public class ItemHolder : MonoBehaviour
    {
        #region Values

        [SerializeField] private Item item;

        #endregion

        #region Build In States
        
        private void OnCollisionEnter(Collision collision)
        {
            if (this.item is not IThrowableItem throwableItem) return;

            throwableItem.OnCollision(collision);
        }

        #endregion
    }
}