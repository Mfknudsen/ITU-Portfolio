#region Packages

using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

#endregion

namespace Runtime.Systems
{
    public abstract class Manager : ScriptableObject
    {
        #region Values

        [ShowInInspector, ReadOnly] protected bool ready;

        #endregion


#if UNITY_EDITOR

        #region Build In States

        private void OnEnable() =>
            EditorApplication.playModeStateChanged += this.ResetManager;

        private void OnDisable() =>
            EditorApplication.playModeStateChanged -= this.ResetManager;

        #endregion

#endif


        #region Getters

        public bool Ready => this.ready;

        #endregion

#if UNITY_EDITOR

        #region Internal

        private void ResetManager(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode)
                return;

            this.ready = false;
            
            this.OnManagerDisabled();
        }

        protected virtual void OnManagerDisabled()
        {
        }

        #endregion

#endif
    }
}