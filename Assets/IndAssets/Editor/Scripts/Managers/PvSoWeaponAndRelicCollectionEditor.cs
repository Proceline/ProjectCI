using UnityEditor;
using UnityEngine;
using IndAssets.Scripts.Managers;
using IndAssets.Scripts.Weapons;
using IndAssets.Scripts.Passives.Relics;
using System.Collections.Generic;

namespace IndAssets.Editor.Scripts.Managers
{
    [CustomEditor(typeof(PvSoWeaponAndRelicCollection))]
    public class PvSoWeaponAndRelicCollectionEditor : UnityEditor.Editor
    {
        private SerializedProperty _weaponsProperty;
        private SerializedProperty _relicsProperty;

        private void OnEnable()
        {
            _weaponsProperty = serializedObject.FindProperty("weapons");
            _relicsProperty = serializedObject.FindProperty("relics");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Weapon and Relic Collection Manager", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Scan buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Scan All Weapons", GUILayout.Height(30)))
            {
                ScanWeapons();
            }
            
            if (GUILayout.Button("Scan All Relics", GUILayout.Height(30)))
            {
                ScanRelics();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(10);

            // Display collected items
            EditorGUILayout.LabelField($"Weapons Count: {_weaponsProperty.arraySize}", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_weaponsProperty, true);
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField($"Relics Count: {_relicsProperty.arraySize}", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_relicsProperty, true);

            serializedObject.ApplyModifiedProperties();
        }

        private void ScanWeapons()
        {
            List<PvSoWeaponData> foundWeapons = new List<PvSoWeaponData>();
            
            // Find all PvSoWeaponData assets in Assets folder
            string[] guids = AssetDatabase.FindAssets("t:PvSoWeaponData", new[] { "Assets" });
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PvSoWeaponData weapon = AssetDatabase.LoadAssetAtPath<PvSoWeaponData>(path);
                if (weapon != null)
                {
                    foundWeapons.Add(weapon);
                }
            }
            
            // Update the serialized property
            _weaponsProperty.ClearArray();
            for (int i = 0; i < foundWeapons.Count; i++)
            {
                _weaponsProperty.InsertArrayElementAtIndex(i);
                _weaponsProperty.GetArrayElementAtIndex(i).objectReferenceValue = foundWeapons[i];
            }
            
            serializedObject.ApplyModifiedProperties();
            
            UnityEngine.Debug.Log($"Scanned and found {foundWeapons.Count} weapon assets.");
        }

        private void ScanRelics()
        {
            List<PvSoPassiveRelic> foundRelics = new List<PvSoPassiveRelic>();
            
            // Find all PvSoPassiveRelic assets in Assets folder
            string[] guids = AssetDatabase.FindAssets("t:PvSoPassiveRelic", new[] { "Assets" });
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PvSoPassiveRelic relic = AssetDatabase.LoadAssetAtPath<PvSoPassiveRelic>(path);
                if (relic != null)
                {
                    foundRelics.Add(relic);
                }
            }
            
            // Update the serialized property
            _relicsProperty.ClearArray();
            for (int i = 0; i < foundRelics.Count; i++)
            {
                _relicsProperty.InsertArrayElementAtIndex(i);
                _relicsProperty.GetArrayElementAtIndex(i).objectReferenceValue = foundRelics[i];
            }
            
            serializedObject.ApplyModifiedProperties();
            
            UnityEngine.Debug.Log($"Scanned and found {foundRelics.Count} relic assets.");
        }
    }
}
