using Runtime.Settings;
using UnityEngine;

namespace Runtime.UI.Pause.OptionInput
{
    public class InvertMouse : MonoBehaviour
    {
        public void UpdateX(bool set)
        {
            GameplaySetting.SetInvert(set, "X");
        }
        
        public void UpdateY(bool set)
        {
            GameplaySetting.SetInvert(set, "Y");
        }
    }
}
