#if UNITY_EDITOR

#region Libraries

using Runtime.World.Overworld;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;

#endregion

namespace Runtime.Editor.Tools
{
    public static class WorldTools
    {
        #region Internal

        [MenuItem("Tools/Mfknudsen/Setup World Scene")]
        private static void SetupNewScene()
        {
            GameObject obj = new GameObject
            {
                name = "'Name' - Tile Manager"
            };

            obj.AddComponent<TileSubController>();
            obj.AddComponent<NavMeshSurface>();
        }

        #endregion
    }
}
#endif