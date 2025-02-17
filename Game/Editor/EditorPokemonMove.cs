#region Packages

using System.Collections.Generic;
using Runtime.Battle.Actions;
using Runtime.Pokémon;
using Runtime.Pokémon.Conditions;
using Runtime.Pokémon.Conditions.Non_Volatiles;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

#endregion

namespace Editor
{
    [CustomEditor(typeof(PokemonMove))]
    public sealed class EditorPokemonMove : OdinEditor
    {
        #region Values

        private PokemonMove script;
        private Color standard = new(0.2f, 0.2f, 0.2f);
        private GUIStyle headerStyle, subheaderStyle, textStyle;
        private bool showContest;

        //Assets
        private List<Type> types;
        private string[] typeNames;
        private List<Condition> conditions;
        private string[] conditionNames;

        #endregion

        protected override void OnEnable()
        {
            //Types
            string[] allTypeAssets = AssetDatabase.FindAssets("t:Type");
            this.types = new List<Type>();
            this.typeNames = new string[allTypeAssets.Length + 1];

            this.types.Add(null);
            this.typeNames[0] = "Null";
            for (int i = 0; i < allTypeAssets.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(allTypeAssets[i]);
                Type typeAsset = AssetDatabase.LoadAssetAtPath<Type>(assetPath);
                this.types.Add(typeAsset);
                this.typeNames[i + 1] = typeAsset.GetTypeName().ToString();
            }

            //Conditions
            string[] allConditionAssets = AssetDatabase.FindAssets("t:Condition");
            this.conditions = new List<Condition>();
            this.conditionNames = new string[allConditionAssets.Length + 1];

            this.conditions.Add(null);
            this.conditionNames[0] = "Null";
            for (int i = 0; i < allConditionAssets.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(allConditionAssets[i]);
                Condition typeAsset = AssetDatabase.LoadAssetAtPath<Condition>(assetPath);

                if (typeAsset is FaintedCondition)
                    continue;

                this.conditions.Add(typeAsset);
                this.conditionNames[i + 1] = typeAsset.GetConditionName();
            }
        }

        public override void OnInspectorGUI()
        {
            this.standard = GUI.color;
            this.script = (PokemonMove)this.target;
            this.headerStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 30 };
            this.subheaderStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 25 };
            this.textStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 20 };

            this.ShowBattle();

            GUILayout.Space(10);

            this.ShowTarget();

            GUILayout.Space(10);

            this.ShowStatus();

            GUILayout.Space(10);

            this.ShowContests();

            GUILayout.Space(10);

            base.OnInspectorGUI();

            EditorUtility.SetDirty(this.script);
        }

        private void ShowBattle()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Battles", this.headerStyle);

            //Type
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(150));
            GUILayout.Label("Type", this.textStyle, GUILayout.Height(30), GUILayout.Width(150));
            EditorGUILayout.EndVertical();

            if (this.script.GetMoveType() != null)
                GUI.backgroundColor = this.script.GetMoveType().GetTypeColor();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = this.standard;
            int choice = EditorGUILayout.Popup(this.types.IndexOf(this.script.GetMoveType()), this.typeNames,
                this.textStyle,
                GUILayout.Height(30)
            );
            this.script.SetType(this.types[choice]);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            //Category
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(150));
            GUILayout.Label("Category", this.textStyle, GUILayout.Height(30), GUILayout.Width(150));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            this.script.SetCategory((Category)EditorGUILayout.EnumPopup(this.script.GetCategory(), this.textStyle,
                GUILayout.Height(30)
            ));
            this.script.SetType(this.types[choice]);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            //PP
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(150));
            GUILayout.Label("PP", this.textStyle, GUILayout.Height(30), GUILayout.Width(150));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            this.script.SetStartPP(EditorGUILayout.IntField(this.script.GetStartPP(), this.textStyle,
                GUILayout.Height(30)
            ));
            EditorGUILayout.EndVertical();

            // - Max
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(100));
            GUILayout.Label("Max", this.textStyle, GUILayout.Height(30), GUILayout.Width(100));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(100));
            this.script.SetMaxPP(EditorGUILayout.IntField(this.script.GetMaxPP(), this.textStyle,
                GUILayout.Height(30)
            ));
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            //Power
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(150));
            GUILayout.Label("Power", this.textStyle, GUILayout.Height(30), GUILayout.Width(150));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            this.script.SetPower(EditorGUILayout.IntField(this.script.GetPower(), this.textStyle,
                GUILayout.Height(30)
            ));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            //Accuracy
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(150));
            GUILayout.Label("Accuracy", this.textStyle, GUILayout.Height(30), GUILayout.Width(150));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            this.script.SetAccuracy(EditorGUILayout.IntField(this.script.GetAccuracy(), this.textStyle,
                GUILayout.Height(30)
            ));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            //Affected
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            string[] affectText = { "Contact", "Protect", "Magic Coat", "Snatch", "Mirror Move", "King's Rock" };
            bool[] truth = this.script.GetAffected();

            for (int i = 0; i < affectText.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                truth[i] = EditorGUILayout.Toggle(truth[i], GUILayout.Width(30));
                GUILayout.Label(affectText[i]);
                EditorGUILayout.EndHorizontal();
            }

            this.script.SetAffected(truth);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private void ShowTarget()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Target", this.headerStyle);

            // ReSharper disable once IdentifierTypo
            bool[] targetable = this.script.GetTargetable();

            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < 3; i++)
            {
                GUI.backgroundColor = targetable[i] ? Color.green : Color.red;

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUI.backgroundColor = this.standard;

                GUILayout.FlexibleSpace();
                GUILayout.Label("Foe", this.subheaderStyle, GUILayout.Width(50));
                targetable[i] = EditorGUILayout.Toggle(
                    targetable[i],
                    GUILayout.Width(15)
                );
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = targetable[5] ? Color.green : Color.red;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUI.backgroundColor = this.standard;

            GUILayout.FlexibleSpace();
            GUILayout.Label("Self", this.subheaderStyle, GUILayout.Width(50));
            targetable[5] = EditorGUILayout.Toggle(
                targetable[5],
                GUILayout.Width(15)
            );
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            for (int i = 3; i < 5; i++)
            {
                GUI.backgroundColor = targetable[i] ? Color.green : Color.red;

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUI.backgroundColor = this.standard;

                GUILayout.FlexibleSpace();
                GUILayout.Label("Ally", this.subheaderStyle, GUILayout.Width(50));
                targetable[i] = EditorGUILayout.Toggle(
                    targetable[i],
                    GUILayout.Width(15)
                );
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            this.script.SetTargetable(targetable);

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Hit Type", this.textStyle, GUILayout.Height(35));
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            this.script.SetHitType((HitType)EditorGUILayout.EnumPopup(this.script.GetHitType(), this.textStyle,
                GUILayout.Height(35)
            ));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void ShowStatus()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            if (EditorGUILayout.Toggle(false, GUILayout.Width(15)))
            {
                this.script.SetHasStatus(!this.script.GetHasStatus());
            }

            GUILayout.Label(this.script.GetHasStatus() ? "Disable Status" : "Enable Status");

            EditorGUILayout.EndHorizontal();

            if (this.script.GetHasStatus())
            {
                GUILayout.Label("Status", this.headerStyle);

                //Condition
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(225));
                GUILayout.Label("Condition", this.textStyle, GUILayout.Height(35), GUILayout.Width(200));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                int choice = EditorGUILayout.Popup(this.conditions.IndexOf(this.script.GetStatusCondition()),
                    this.conditionNames, this.textStyle,
                    GUILayout.Height(35)
                );

                this.script.SetStatusCondition(this.conditions[choice]);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                //Change
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(225));
                GUILayout.Label("Apply Change", this.textStyle, GUILayout.Height(35));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                choice = EditorGUILayout.IntField(this.script.GetApplyChance(), this.textStyle,
                    GUILayout.Height(35)
                );

                choice = Mathf.Clamp(choice, 0, 100);

                this.script.SetApplyChance(choice);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void ShowContests()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.Toggle(false, GUILayout.Width(15)))
            {
                this.showContest = !this.showContest;
            }

            GUILayout.Label(this.showContest ? "Hide Contests" : "Show Contests");

            EditorGUILayout.EndHorizontal();

            if (this.showContest)
            {
                string[] headers = { "Contests", "Super Contests", "Spectacular Contests" };
                string[] labels = { "Condition", "Appeal", "Jam" };

                for (int j = 0; j < 3; j++)
                {
                    int[] values;

                    if (j == 0)
                        values = this.script.GetNormalContests();
                    else if (j == 1)
                        values = this.script.GetSuperContests();
                    else
                        values = this.script.GetSpectacularContests();

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Label(headers[j], this.headerStyle);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(200));
                    GUILayout.Label(labels[0], this.subheaderStyle, GUILayout.Height(40));
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    values[0] = (int)(Contest)EditorGUILayout.EnumPopup(
                        (Contest)values[0], this.subheaderStyle,
                        GUILayout.Height(40)
                    );
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndHorizontal();

                    for (int i = 1; i < 3; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(200));
                        GUILayout.Label(labels[i], this.subheaderStyle, GUILayout.Height(40));
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        values[i] = EditorGUILayout.IntField(
                            values[i], this.subheaderStyle,
                            GUILayout.Height(40)
                        );
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();

                    if (j == 0)
                        this.script.SetNormalContests(values);
                    else if (j == 1)
                        this.script.SetSuperContests(values);
                    else
                        this.script.SetSpectacularContests(values);
                }
            }

            EditorGUILayout.EndVertical();
        }
    }
}