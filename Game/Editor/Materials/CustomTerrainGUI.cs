#region Libraries

using UnityEditor;
using UnityEngine;

#endregion

// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedType.Global
public class CustomTerrainGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        MaterialProperty cascade0 = FindProperty("_Cascade0", properties),
            cascade1 = FindProperty("_Cascade1", properties),
            cascade2 = FindProperty("_Cascade2", properties),
            cascade3 = FindProperty("_Cascade3", properties);

        EditorGUILayout.LabelField("Distance:");
        cascade0.floatValue = Mathf.Clamp(cascade0.floatValue, 0, Mathf.Infinity);
        materialEditor.ShaderProperty(cascade0, cascade0.displayName);

        cascade1.floatValue = Mathf.Clamp(cascade1.floatValue, cascade0.floatValue, Mathf.Infinity);
        materialEditor.ShaderProperty(cascade1, cascade1.displayName);

        cascade2.floatValue = Mathf.Clamp(cascade2.floatValue, cascade1.floatValue, Mathf.Infinity);
        materialEditor.ShaderProperty(cascade2, cascade2.displayName);

        cascade3.floatValue = Mathf.Clamp(cascade3.floatValue, cascade2.floatValue, Mathf.Infinity);
        materialEditor.ShaderProperty(cascade3, cascade3.displayName);
    }
}