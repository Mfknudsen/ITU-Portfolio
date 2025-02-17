#region Packages

using UnityEditor;

#endregion

namespace Editor.Tools
{
    public sealed class ChatEditorWindow : EditorWindow
    {
        [MenuItem("Window/Mfknudsen/Chat")]
        private static void Init()
        {
            ChatEditorWindow window = GetWindow<ChatEditorWindow>(true, "Chat Editor");
            window.Show();
        }
    }
}