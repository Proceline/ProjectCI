using UnityEditor;
using ProjectCI.CoreSystem.Editor.TacticRpgTool;
using ProjectCI.CoreSystem.Runtime.Abilities;
using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.CoreSystem.Editor.Abilities
{
    [CustomEditor(typeof(PvSoUnitAbility))]
    public class PvSoUnitAbilityEditor : UnitAbilityEditor
    {
        private static string pathOfEnumMarks = "Assets/IndAssets/_1stPartyAssets/AnimationAssets/AnimationEnumMarks";
        
        // Cache for animation assets and names
        private List<UnitAbilityAnimation> animationAssets = new List<UnitAbilityAnimation>();
        private string[] animationNames;
        private int selectedIndex = 0;
        
        private void OnEnable()
        {
            LoadAnimationAssets();
        }
        
        private void LoadAnimationAssets()
        {
            animationAssets.Clear();
            
            // Find all UnitAbilityAnimation assets in the specified path
            string[] guids = AssetDatabase.FindAssets("t:UnitAbilityAnimation", new[] { pathOfEnumMarks });
            
            // Add "None" option
            animationAssets.Add(null);
            
            // Load all found assets and filter for UnitAbilityAnimation types
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                UnitAbilityAnimation asset = AssetDatabase.LoadAssetAtPath<UnitAbilityAnimation>(path);
                if (asset != null)
                {
                    animationAssets.Add(asset);
                }
            }
            
            // Create names array for popup
            animationNames = new string[animationAssets.Count];
            animationNames[0] = "None";
            for (int i = 1; i < animationAssets.Count; i++)
            {
                animationNames[i] = animationAssets[i] != null ? animationAssets[i].name : "Unknown";
            }
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            SerializedProperty abilityAnimationObj = serializedObject.FindProperty("abilityAnimation");
            
            // Draw animation selection dropdown
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Ability Animation", EditorStyles.boldLabel);
            
            // Find current selection index
            ScriptableObject currentAnimation = abilityAnimationObj.objectReferenceValue as ScriptableObject;
            selectedIndex = 0; // Default to "None"
            for (int i = 1; i < animationAssets.Count; i++)
            {
                if (animationAssets[i] == currentAnimation)
                {
                    selectedIndex = i;
                    break;
                }
            }
            
            // Draw popup
            int newSelectedIndex = EditorGUILayout.Popup("Animation Asset", selectedIndex, animationNames);
            
            // Handle selection change
            if (newSelectedIndex != selectedIndex)
            {
                selectedIndex = newSelectedIndex;
                abilityAnimationObj.objectReferenceValue = animationAssets[selectedIndex];
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
} 