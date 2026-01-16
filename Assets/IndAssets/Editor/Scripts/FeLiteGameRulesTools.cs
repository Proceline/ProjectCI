using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.Abilities;

namespace ProjectCI.Editor.Tools
{
    public static class FeLiteGameRulesTools
    {
        private const string MenuPath = "ProjectCI Tools/GameRules/Reset All Abilities";

        [MenuItem(MenuPath)]
        public static void ResetAndPopulateAllAbilities()
        {
            // 1. Find all PvSoUnitAbility assets
            var abilityGuids = AssetDatabase.FindAssets("t:PvSoUnitAbility");
            var abilities = new List<PvSoUnitAbility>();

            foreach (var guid in abilityGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var ability = AssetDatabase.LoadAssetAtPath<PvSoUnitAbility>(path);
                if (ability != null)
                {
                    abilities.Add(ability);
                }
            }

            Debug.Log($"Found {abilities.Count} abilities in project.");

            // 2. Find all FeLiteGameRules assets
            var ruleGuids = AssetDatabase.FindAssets("t:FeLiteGameRules");
            if (ruleGuids.Length == 0)
            {
                Debug.LogError("No FeLiteGameRules assets found in the project.");
                return;
            }

            // 3. Update each GameRules asset
            int updatedCount = 0;
            foreach (var guid in ruleGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var rules = AssetDatabase.LoadAssetAtPath<FeLiteGameRules>(path);
                
                if (rules == null) continue;

                Undo.RecordObject(rules, "Reset All Abilities");

                var serializedObj = new SerializedObject(rules);
                var allAbilitiesProp = serializedObj.FindProperty("allAbilities");

                if (allAbilitiesProp == null)
                {
                    Debug.LogError($"Field 'allAbilities' not found in {rules.name}. Check if it is serialized.");
                    continue;
                }

                allAbilitiesProp.ClearArray();
                
                for (int i = 0; i < abilities.Count; i++)
                {
                    allAbilitiesProp.InsertArrayElementAtIndex(i);
                    var element = allAbilitiesProp.GetArrayElementAtIndex(i);
                    element.objectReferenceValue = abilities[i];
                }

                serializedObj.ApplyModifiedProperties();
                EditorUtility.SetDirty(rules);
                updatedCount++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Successfully reset 'allAbilities' for {updatedCount} FeLiteGameRules asset(s).");
        }
    }
}
