using UnityEngine;

namespace Runtime.Menu
{
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager instance;
        [SerializeField] private SettingsData data;

        private void Start()
        {
            if (instance)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
                Destroy(this.gameObject);
        }
    }
}
