#region Packages

using Runtime.Systems.UI;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.UI.Battle
{
    public class BattleUI : MonoBehaviour
    {
        #region Values

        [SerializeField, Required] private UIManager uiManager;

        #endregion

        #region Build In States

        private void Start() =>
            this.uiManager.SetBattleUI(this.transform);

        #endregion
    }
}