#region Packages

using UnityEngine;

#endregion

namespace Runtime.Items
{
    public interface IThrowableItem
    {
        public void OnCollision(Collision collision);
    }
}