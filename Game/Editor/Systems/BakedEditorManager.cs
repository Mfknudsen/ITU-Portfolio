#region Libraries

using UnityEditor;

#endregion

namespace Editor.Systems
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    public static class BakedEditorManager
    {
        #region Values

        private static bool isBakeRunning;

        #endregion

        #region Getters

        public static bool IsBakeRunning =>
            isBakeRunning;

        #endregion

        #region Setters

        public static void SetRunning(bool set) =>
            isBakeRunning = set;

        #endregion
    }
#endif
}