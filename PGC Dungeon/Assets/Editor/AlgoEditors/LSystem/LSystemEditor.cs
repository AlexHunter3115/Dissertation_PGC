
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NewLSystem))]

public class LSystemEditor : Editor
{
    bool showRules = false;


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        NewLSystem ruleDec = (NewLSystem)target;


        #region explanation


        GeneralUtil.SpacesUILayout(4);

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



        GeneralUtil.SpacesUILayout(4);


        #endregion





        if (GUILayout.Button("New rule Set"))
        {
            var ruleSet = ScriptableObject.CreateInstance<LSystemRuleObj>();

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
                AssetDatabase.Refresh();
            }

            if (!AssetDatabase.IsValidFolder("Assets/Resources/L-systemRuleSets"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "L-systemRuleSets");
                AssetDatabase.Refresh();
            }

            AssetDatabase.CreateAsset(ruleSet, $"Assets/Resources/L-systemRuleSets/{ruleDec.fileName}.asset");
            AssetDatabase.SaveAssets();
        }


        if (GUILayout.Button("Load Rule Set"))
        {
            var ruleSet = Resources.Load<LSystemRuleObj>("L-systemRuleSets/" +ruleDec.fileName);

            ruleDec.A_dist = ruleSet.A_Length;
            ruleDec.B_dist = ruleSet.B_Length;
            ruleDec.C_dist = ruleSet.C_Length;

            ruleDec.A_RuleSet = ruleSet.A_RuleSet;
            ruleDec.B_RuleSet = ruleSet.B_RuleSet;
            ruleDec.C_RuleSet = ruleSet.C_RuleSet;
            ruleDec.N_RuleSet = ruleSet.Nsign_RuleSet;
            ruleDec.P_RuleSet = ruleSet.Psign_RuleSet;
            ruleDec.S_RuleSet = ruleSet.S_RuleSet;
            ruleDec.L_RuleSet = ruleSet.L_RuleSet;
        }

        if (GUILayout.Button("Run Iteration"))
        {
            ruleDec.RunIteration();
        }
    }
}

