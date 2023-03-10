using UnityEngine;
using UnityEditor;

namespace DungeonForge
{
    [CustomEditor(typeof(LSystem))]

    public class LSystemEditor : Editor
    {
        bool showRules = false;
        string saveMapFileName = "";

        public override void OnInspectorGUI()
        {

            LSystem ruleDec = (LSystem)target;


            #region explanation


            DFGeneralUtil.SpacesUILayout(4);

            showRules = EditorGUILayout.BeginFoldoutHeaderGroup(showRules, "Instructions");

            if (showRules)
            {
                GUILayout.TextArea("You have choosen l system");

            }

            if (!Selection.activeTransform)
            {
                showRules = false;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();



            DFGeneralUtil.SpacesUILayout(4);


            #endregion

            if (!ruleDec.generated) 
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("New rule Set"))
                {
                    var asset = CreateInstance<LSystemRuleObj>();

                    if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    {
                        AssetDatabase.CreateFolder("Assets", "Resources");
                        AssetDatabase.Refresh();
                    }

                    if (!AssetDatabase.IsValidFolder("Assets/Resources/Resources_Algorithms"))
                    {
                        AssetDatabase.CreateFolder("Assets/Resources", "Resources_Algorithms");
                        AssetDatabase.Refresh();
                    }


                    if (!AssetDatabase.IsValidFolder("Assets/Resources/Resources_Algorithms/L_system_Rule_Sets"))
                    {
                        AssetDatabase.CreateFolder("Assets/Resources/Resources_Algorithms", "L_system_Rule_Sets");
                        AssetDatabase.Refresh();
                    }

                    AssetDatabase.CreateAsset(asset, $"Assets/Resources/Resources_Algorithms/L_system_Rule_Sets/{(ruleDec.fileName == string.Empty ? "LSystemRuleSet" : ruleDec.fileName)}.asset");
                    AssetDatabase.SaveAssets();
                }

                if (GUILayout.Button("Load Rule Set"))
                {
                    var ruleSet = Resources.Load<LSystemRuleObj>("Resources_Algorithms/L_system_Rule_Sets/" + ruleDec.fileName);

                    if (ruleSet != null)
                    {
                        ruleDec.A_dist = ruleSet.A_Length;
                        ruleDec.B_dist = ruleSet.B_Length;
                        ruleDec.C_dist = ruleSet.C_Length;

                        ruleDec.A_RuleSet = ruleSet.A_RuleSet;
                        ruleDec.B_RuleSet = ruleSet.B_RuleSet;
                        ruleDec.C_RuleSet = ruleSet.C_RuleSet;
                        ruleDec.N_RuleSet = ruleSet.negativeSignRuleSet;
                        ruleDec.P_RuleSet = ruleSet.positiveSignRuleSet;
                        ruleDec.S_RuleSet = ruleSet.S_RuleSet;
                        ruleDec.L_RuleSet = ruleSet.L_RuleSet;

                        ruleDec.roomMacros = ruleSet.roomGenerationMacros;
                        ruleDec.loaded = true;
                    }
                    else
                    {
                        ruleDec.loaded = false;
                    }

                }

                DFGeneralUtil.SpacesUILayout(2);

                EditorGUI.BeginDisabledGroup(ruleDec.loaded == false || ruleDec.axium == string.Empty);

                if (GUILayout.Button(new GUIContent() { text = "Run algorithm", tooltip = (ruleDec.loaded == false && ruleDec.axium == string.Empty) ? "run the algorithm with the given ruleset and Axium" : "There is something that you missed, either the axium is empty or nothing was loaded" }))
                {
                    ruleDec.RunIteration();
                    ruleDec.generated = true;
                }

                EditorGUI.EndDisabledGroup();
            }
            else 
            {
                if (GUILayout.Button(new GUIContent() { text = "Restart", tooltip = "" }))
                {
                    ruleDec.PcgManager.Restart();
                    ruleDec.generated = false;
                }

                 DFGeneralUtil.SpacesUILayout(2);

                 DFGeneralUtil.GenerateMeshEditorSection(ruleDec.PcgManager, saveMapFileName, out saveMapFileName);
            }
        }
    }
}