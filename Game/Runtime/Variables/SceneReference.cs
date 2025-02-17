using System;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Runtime.Variables
{
    /// <summary>
    /// A wrapper that provides the means to safely serialize Scene Asset References.
    /// </summary>
    [Serializable]
    public sealed class SceneReference : ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        // What we use in editor to select the scene
        [SerializeField] private Object sceneAsset;
        private bool IsValidSceneAsset
        {
            get
            {
                if (!this.sceneAsset) return false;

                return this.sceneAsset is SceneAsset;
            }
        }
#endif

        public bool IsEmpty => this.sceneAsset == null;


        // This should only ever be set during serialization/deserialization!
        [SerializeField]
        private string scenePath = string.Empty;

        // Use this when you want to actually have the scene path
        public string ScenePath
        {
            get
            {
#if UNITY_EDITOR
                // In editor we always use the asset's path
                return this.GetScenePathFromAsset();
#else
            // At runtime we rely on the stored path value which we assume was serialized correctly at build time.
            // See OnBeforeSerialize and OnAfterDeserialize
            return scenePath;
#endif
            }
            set
            {
                this.scenePath = value;
#if UNITY_EDITOR
                this.sceneAsset = this.GetSceneAssetFromPath();
#endif
            }
        }

        public static implicit operator string(SceneReference sceneReference) =>
            sceneReference.ScenePath;

        // Called to prepare this data for serialization. Stubbed out when not in editor.
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            this.HandleBeforeSerialize();
#endif
        }

        // Called to set up data for deserialization. Stubbed out when not in editor.
        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            // We sadly cannot touch assetdatabase during serialization, so defer by a bit.
            EditorApplication.update += this.HandleAfterDeserialize;
#endif
        }

#if UNITY_EDITOR
        private SceneAsset GetSceneAssetFromPath()
        {
            return string.IsNullOrEmpty(this.scenePath) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(this.scenePath);
        }

        private string GetScenePathFromAsset()
        {
            return this.sceneAsset == null ? string.Empty : AssetDatabase.GetAssetPath(this.sceneAsset);
        }

        private void HandleBeforeSerialize()
        {
            // Asset is invalid but have Path to try and recover from
            if (this.IsValidSceneAsset == false && string.IsNullOrEmpty(this.scenePath) == false)
            {
                this.sceneAsset = this.GetSceneAssetFromPath();
                if (this.sceneAsset == null) this.scenePath = string.Empty;

                EditorSceneManager.MarkAllScenesDirty();
            }
            // Asset takes precendence and overwrites Path
            else
            {
                this.scenePath = this.GetScenePathFromAsset();
            }
        }

        private void HandleAfterDeserialize()
        {
            EditorApplication.update -= this.HandleAfterDeserialize;
            // Asset is valid, don't do anything - Path will always be set based on it when it matters
            if (this.IsValidSceneAsset) return;

            // Asset is invalid but have path to try and recover from
            if (string.IsNullOrEmpty(this.scenePath)) return;

            this.sceneAsset = this.GetSceneAssetFromPath();
            // No asset found, path was invalid. Make sure we don't carry over the old invalid path
            if (!this.sceneAsset) this.scenePath = string.Empty;

            if (!Application.isPlaying) EditorSceneManager.MarkAllScenesDirty();
        }
#endif
    }
}