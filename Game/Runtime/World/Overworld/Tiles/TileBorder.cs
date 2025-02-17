#region Packages

using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.World.Overworld.Tiles
{
    public class TileBorder : MonoBehaviour
    {
        #region Values

        [SerializeField, Required] private TileManager tileManager;
        [SerializeField] private string fromName, toName;

        #endregion

        #region In

        public void Trigger(bool exitCurrentTile)
        {
            this.tileManager.SetCurrentSubTile(exitCurrentTile ? this.toName : this.fromName);
        }

        #endregion
    }
}