#region Packages

using Runtime.Settings;
using UnityEngine;

#endregion

namespace Runtime.UI
{
    public class OptionsUI : MonoBehaviour
    {
        #region Values

        #endregion

        #region In

        public void ApplySettings()
        {
            #region Screen

            Vector2 res = Setting.Resolutions[Setting.ResolutionIndex];
            FullScreenMode mode = Setting.ScreenSetting;
            Screen.SetResolution((int)res.x, (int)res.y, mode);

            #endregion
        }

        public void SetDifficultly(Difficultly set)
        {
            Setting.Difficultly = set;
        }

        #region Screen

        public void SetResolution(int i)
        {
            Setting.ResolutionIndex += i;
            if (Setting.ResolutionIndex < 0)
                Setting.ResolutionIndex = Setting.Resolutions.Length - 1;
            else if (Setting.ResolutionIndex >= Setting.Resolutions.Length)
                Setting.ResolutionIndex = 0;
        }

        #endregion

        #endregion
    }
}