#region Packages

using UnityEngine;

#endregion

namespace Runtime.World.Overworld.Tiles
{
    public class TileBorderTrigger : MonoBehaviour
    {
        #region Values

        [SerializeField] private bool exitCurrent;
        
        private TileBorder border;
        
        #endregion

        #region Build In States

        private void Awake()
        {
            this.border = this.transform.parent.GetComponent<TileBorder>();

            if (this.border == null)
            {
                Destroy(this.gameObject);
                Debug.LogError("Tile Border Trigger must be child of Tile Border");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if(!other.transform.root.name.Equals("Player")) return;

            this.border.Trigger(this.exitCurrent);
        }

        #endregion
    }
}
