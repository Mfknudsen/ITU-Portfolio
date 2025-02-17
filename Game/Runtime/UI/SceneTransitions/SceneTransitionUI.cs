using UnityEngine;

namespace Runtime.UI.SceneTransitions
{
    public class SceneTransitionUI : MonoBehaviour
    {
        public static SceneTransitionUI instance;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this.gameObject);
        }

        public GameObject InstantiateObject(GameObject obj)
        {
            return Instantiate(obj, this.transform);
        }
    }
}