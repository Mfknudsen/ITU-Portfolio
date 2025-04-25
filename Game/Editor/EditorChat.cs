#region Packages

using Runtime.Communication;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif
using UnityEditor;
using UnityEngine;

#endregion

namespace Editor
{
#if UNITY_EDITOR
    [CustomEditor(typeof(Chat), true)]
    public sealed class EditorChat : OdinEditor
    {
        #region Values

        private Chat script;

        private GUIStyle textStyle;

        #endregion

        protected override void OnEnable() =>
            this.script = this.target as Chat;

        public override void OnInspectorGUI()
        {
            if (this.script == null) return;

            this.ShowList();

            base.OnInspectorGUI();
        }

        private void ShowList()
        {
            if (GUILayout.Button("Create New"))
                this.script.CreateNew();

            for (int i = 0; i < this.script.GetListCount; i++)
            {
                EditorGUILayout.BeginHorizontal("Box");

                //Hide after and time till next
                EditorGUILayout.BeginVertical(GUILayout.Width(30));

                this.script.SetTimeToNextByIndex(i,
                    EditorGUILayout.FloatField(this.script.GetTimeToNextByIndex(i)));

                EditorGUILayout.EndVertical();

                this.script.SetTextByIndex(i,
                    EditorGUILayout.TextField(this.script.GetTextByIndex(i), GUILayout.Height(40)));

                if (GUILayout.Button("X", GUILayout.Width(20)))
                    this.script.DeleteByIndex(i);

                EditorGUILayout.EndHorizontal();
            }
        }
    }
#endif
}