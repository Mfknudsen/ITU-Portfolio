#region Packages

using UnityEditor;

#endregion

namespace Editor.Tools
{
#if UNITY_EDITOR
    public sealed class PokemonEditorWindow : EditorWindow
    {
        [MenuItem("Window/Mfknudsen/Pokemon")]
        private static void Init()
        {
            PokemonEditorWindow window = GetWindow<PokemonEditorWindow>(true, "Pokemon Editor");
            window.Show();
        }
    }
#endif
}