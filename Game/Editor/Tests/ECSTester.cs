using UnityEngine;

namespace Editor.Tests
{
    public sealed class EcsTester : MonoBehaviour
    {
        public GameObject ecs;

        private void Update()
        {
            Debug.Log(this.ecs == null);
        }
    }
}