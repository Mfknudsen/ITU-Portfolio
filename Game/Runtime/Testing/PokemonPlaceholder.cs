#region Libraries

using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

#endregion

namespace Runtime.Testing
{
    public sealed class PokemonPlaceholder : MonoBehaviour
    {
        [SerializeField, Required] private TextMeshPro textMesh;

        private void Update()
        {
            if (Camera.main == null)
                return;

            Vector3 position = this.transform.position;
            Vector3 target = Camera.main.transform.position - position;
            target = new Vector3(target.x, 0, target.z);
            target = position - target;

            this.transform.LookAt(target);
        }

        public void SetText(string t) =>
            this.textMesh.text = t + "\nPlaceholder";
    }
}