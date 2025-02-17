#region Packages

using Runtime.ScriptableVariables.Structs.ListVariables;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Linq;
using UnityEngine;

#endregion

namespace Runtime.World.Overworld.Interactions
{
    public sealed class InteractItem : MonoBehaviour
    {
        #region Values

        [SerializeField, Required] private InteractItemListGenericVariable itemListGenericVariable;

        [SerializeField] private float iconOffset, triggerOffset;

        [SerializeField] private MonoBehaviour[] onTrigger;

        [SerializeField] private float radius;

        #endregion

        #region Build In States

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 position = this.transform.position;
            Gizmos.DrawWireSphere(position + Vector3.up * this.triggerOffset, this.radius);
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(position + Vector3.up * this.iconOffset, .2f);
        }

        private void OnValidate()
        {
            if (this.onTrigger == null)
                return;

            if (this.onTrigger.Length > 0)
            {
                this.onTrigger = this.onTrigger
                    .OfType<IInteractable>()
                    .Select(i => i as MonoBehaviour)
                    .ToArray();
            }
        }
#endif

        private void OnEnable() =>
            this.itemListGenericVariable.AddElement(this);

        private void OnDisable() =>
            this.itemListGenericVariable.RemoveElement(this);

        #endregion

        #region Getters

        public Vector3 GetPosition() =>
            this.transform.position + Vector3.up * this.triggerOffset;

        public Vector3 GetIconPosition() =>
            this.transform.position + Vector3.up * this.iconOffset;

        public float GetRadius() => this.radius;

        #endregion

        #region In

        public void Trigger()
        {
            this.onTrigger
                .Select(script =>
                    script as IInteractable)
                .ForEach(i =>
                    i.InteractTrigger());
        }

        #endregion
    }
}