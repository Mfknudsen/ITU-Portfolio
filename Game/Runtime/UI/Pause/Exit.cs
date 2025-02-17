#region Packages

using JetBrains.Annotations;
using UnityEngine;

#endregion

namespace Runtime.UI.Pause
{
    public class Exit : MonoBehaviour
    {
        #region In

        [UsedImplicitly]
        public void Cancel()
        {
            this.gameObject.SetActive(false);
        }

        [UsedImplicitly]
        public void Accept()
        {
            Application.Quit();
        }

        #endregion
    }
}
