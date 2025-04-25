#region Libraries

using UnityEditor;
using UnityEngine;

#endregion

namespace Editor.World.River
{
#if UNITY_EDITOR
    [CustomEditor(typeof(RiverConstructor))]
    public sealed class RiverConstructorEditor : UnityEditor.Editor
    {
        #region Values

        private const KeyCode ADD_KEY_CODE = KeyCode.E,
            REMOVE_KEY_CODE = KeyCode.R;

        private int selectedIndex = -1;
        private float pointRadius;

        #endregion

        #region Build In States

        public override void OnInspectorGUI()
        {
            this.HandleInput();

            base.OnInspectorGUI();
        }

        #endregion

        #region Internal

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                return;
            }

            if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                return;

            if (Input.GetKeyDown(ADD_KEY_CODE))
            {
            }
            else if (Input.GetKeyDown(REMOVE_KEY_CODE))
            {
            }
        }

        #endregion
    }
#endif
}