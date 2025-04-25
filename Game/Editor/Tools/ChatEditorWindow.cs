#region Packages

using UnityEditor;

#endregion

namespace Editor.Tools
{
#if UNITY_EDITOR
    public sealed class ChatEditorWindow : EditorWindow
    {
        [MenuItem("Window/Mfknudsen/Chat")]
        private static void Init()
        {
            ChatEditorWindow window = GetWindow<ChatEditorWindow>(true, "Chat Editor");
            window.Show();
        }
    }
#endif
}