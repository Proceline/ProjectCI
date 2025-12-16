using UnityEditor;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Saving;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using IndAssets.Scripts.Weapons;
using IndAssets.Scripts.Passives.Relics;

namespace IndAssets.Editor.Scripts.Saving
{
    /// <summary>
    /// Custom editor for PvSaveManager
    /// Provides buttons to test UnlockCharacter, AddWeaponInstance, and AddRelicInstance methods
    /// </summary>
    [CustomEditor(typeof(PvSaveManager))]
    public class PvSaveManagerEditor : UnityEditor.Editor
    {
        private PvSoBattleUnitData _unitDataForUnlock;
        private PvSoWeaponData _weaponDataForAdd;
        private PvSoPassiveRelic _relicDataForAdd;

        public override void OnInspectorGUI()
        {
            // Draw default inspector to maintain original functionality
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(10);

            // Get the target object
            PvSaveManager saveManager = (PvSaveManager)target;

            // Unlock Character section
            EditorGUILayout.LabelField("Unlock Character", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _unitDataForUnlock = (PvSoBattleUnitData)EditorGUILayout.ObjectField(
                "Unit Data",
                _unitDataForUnlock,
                typeof(PvSoBattleUnitData),
                false);
            
            EditorGUI.BeginDisabledGroup(_unitDataForUnlock == null);
            if (GUILayout.Button("Unlock Character", GUILayout.Width(150)))
            {
                saveManager.UnlockCharacter(_unitDataForUnlock);
                EditorUtility.SetDirty(saveManager);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Add Weapon Instance section
            EditorGUILayout.LabelField("Add Weapon Instance", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _weaponDataForAdd = (PvSoWeaponData)EditorGUILayout.ObjectField(
                "Weapon Data",
                _weaponDataForAdd,
                typeof(PvSoWeaponData),
                false);
            
            EditorGUI.BeginDisabledGroup(_weaponDataForAdd == null);
            if (GUILayout.Button("Add Weapon", GUILayout.Width(150)))
            {
                saveManager.AddWeaponInstance(_weaponDataForAdd);
                EditorUtility.SetDirty(saveManager);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Add Relic Instance section
            EditorGUILayout.LabelField("Add Relic Instance", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _relicDataForAdd = (PvSoPassiveRelic)EditorGUILayout.ObjectField(
                "Relic Data",
                _relicDataForAdd,
                typeof(PvSoPassiveRelic),
                false);
            
            EditorGUI.BeginDisabledGroup(_relicDataForAdd == null);
            if (GUILayout.Button("Add Relic", GUILayout.Width(150)))
            {
                saveManager.AddRelicInstance(_relicDataForAdd);
                EditorUtility.SetDirty(saveManager);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(5);

            // Save Game section
            EditorGUILayout.LabelField("Save Game", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(!saveManager.IsInitialized);
            if (GUILayout.Button("Save Current Game", GUILayout.Height(30)))
            {
                saveManager.SaveCurrentGame();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
