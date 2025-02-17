#region Packages

using UnityEditor;

#endregion

namespace Editor.Tools
{
    public sealed class PokemonEditorWindow : EditorWindow
    {
        [MenuItem("Window/Mfknudsen/Pokemon")]
        private static void Init()
        {
            PokemonEditorWindow window = GetWindow<PokemonEditorWindow>(true, "Pokemon Editor");
            window.Show();
        }
    }
}
