#region Libraries

using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#endregion

namespace Editor.Tools
{
#if UNITY_EDITOR
    public static class CustomMenu
    {
        [MenuItem("Assets/Create/Shader/HLSL")]
        public static void Create()
        {
            if (!TryGetActiveFolderPath(out string folderPath))
            {
                Debug.LogError("No path found to create HLSL file at");
                return;
            }

            folderPath = Application.streamingAssetsPath.Replace("Assets/StreamingAssets", $"{folderPath}/HLSL");

            const string extension = ".hlsl";

            int i = 0;
            string number = "";
            while (File.Exists($"{folderPath}{number}{extension}"))
            {
                i++;
                number = " " + i;
            }

            string finalPath = $"{folderPath}{number}{extension}";

            File.WriteAllText(finalPath, StartContent);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private static bool TryGetActiveFolderPath(out string path)
        {
            MethodInfo tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod("TryGetActiveFolderPath",
                BindingFlags.Static | BindingFlags.NonPublic);

            object[] args = { null };
            bool found = tryGetActiveFolderPath != null && (bool)tryGetActiveFolderPath.Invoke(null, args);
            path = (string)args[0];

            return found;
        }

        private static string StartContent =>
            "#include\"Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl\"\n\n" +
            "struct Attributes\n{\n    float3 positionOS : POSITION;\n};\n\n" +
            "struct Interpolator\n{\n    float4 positionCS : SV_POSITION;\n};\n\n" +
            "Interpolators Vertex(Attributes input)\n{\n    Interpolator output;\n    return output;\n}\n\n" +
            "float4 Fragment(Interpolators input) : SV_Target\n{\n    return 0;\n}";
    }
#endif
}