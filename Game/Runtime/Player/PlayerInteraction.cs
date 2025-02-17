#region Packages

using System.Collections.Generic;
using System.Linq;
using Runtime.Core;
using Runtime.ScriptableVariables.Structs.ListVariables;
using Runtime.Systems;
using Runtime.World.Overworld.Interactions;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        #region Values

        [SerializeField] private InteractItem focusedInteractable;

        [SerializeField, Required] private InteractItemListGenericVariable itemListGenericVariable;

        [SerializeField] private float radius;

        #endregion

        #region Build In States

        private void OnEnable()
        {
            this.focusedInteractable = null;
            InputManager.Instance.interactInputEvent.AddListener(this.TriggerClosest);
        }

        private void OnDisable()
        {
            this.focusedInteractable = null;
            InputManager.Instance.interactInputEvent.RemoveListener(this.TriggerClosest);
        }

        private void Update() =>
            this.Evaluate();

        #endregion

        #region Getters

        public Vector3 GetFocusedPosition() =>
            this.focusedInteractable == null ? Vector3.zero : this.focusedInteractable.GetIconPosition();

        #endregion

        #region Internal

        private void TriggerClosest()
        {
            if (this.focusedInteractable == null) return;

            this.focusedInteractable.Trigger();
        }

        private void Evaluate()
        {
            Vector3 playerPos = this.transform.position;
            List<InteractItem> interactableInRange = this.itemListGenericVariable.Value
                .Where(i => i.GetPosition().QuickDistanceLessThen(playerPos, this.radius + i.GetRadius())).ToList();

            if (this.focusedInteractable != null)
            {
                if (!interactableInRange.Contains(this.focusedInteractable))
                    this.focusedInteractable = null;
            }

            float dist = this.focusedInteractable == null
                ? Mathf.Infinity
                : Vector3.Distance(playerPos, this.focusedInteractable.GetPosition());

            foreach (InteractItem interactable in interactableInRange)
            {
                if (this.focusedInteractable == interactable) continue;

                float tempDist = Vector3.Distance(playerPos, interactable.GetPosition());

                if (!(tempDist < dist)) continue;

                dist = tempDist;
                this.focusedInteractable = interactable;
            }
        }

        #endregion
    }
}