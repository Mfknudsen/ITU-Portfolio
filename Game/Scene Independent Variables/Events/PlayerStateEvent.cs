#region Libraries

using Runtime.Player;
using Runtime.ScriptableEvents;
using UnityEngine;
using UnityEngine.Events;

#endregion

namespace Runtime.ScriptableVariables.Events
{
    [CreateAssetMenu(menuName = "Variables/Event/Player State")]
    public sealed class PlayerStateEvent : ScriptableVariable<UnityEvent<PlayerState>>, IScriptableEvent<PlayerState>
    {
        #region In

        public void AddListener(UnityAction<PlayerState> action)
        {
            this.Value ??= new UnityEvent<PlayerState>();

            this.Value.AddListener(action);
        }

        public void RemoveListener(UnityAction<PlayerState> action) =>
            this.Value?.RemoveListener(action);

        public void Trigger(PlayerState value) =>
            this.Value?.Invoke(value);

        #endregion
    }
}