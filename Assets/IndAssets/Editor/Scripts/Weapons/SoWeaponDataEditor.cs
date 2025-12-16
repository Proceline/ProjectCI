using IndAssets.Scripts.Weapons;
using UnityEditor;
using UnityEngine;

namespace IndAssets.Editor.Scripts.Weapons
{
    [CustomEditor(typeof(PvSoWeaponData))]
    public class SoWeaponDataEditor : UnityEditor.Editor
    {
        private SerializedProperty _mWeaponNumberIdentifier;
        private SerializedProperty _mWeaponName;
        private SerializedProperty _mDescription;
        private SerializedProperty _mAttributes;

        private void OnEnable()
        {
            _mWeaponName = serializedObject.FindProperty("weaponName");
            _mDescription = serializedObject.FindProperty("description");
            _mAttributes = serializedObject.FindProperty("attributes");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_mWeaponName);
            EditorGUILayout.PropertyField(_mDescription);

            var animatorProperty = serializedObject.FindProperty(PvSoWeaponData.AnimatorPropertyName);
            var abilityProperty = serializedObject.FindProperty(PvSoWeaponData.BindingAbilityPropertyName);
            EditorGUILayout.PropertyField(animatorProperty);
            EditorGUILayout.PropertyField(abilityProperty);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PvSoWeaponData.weaponPrefab)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PvSoWeaponData.prefabLocalRotation)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PvSoWeaponData.prefabLocalPosition)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PvSoWeaponData.prefabLocalScale)));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Attributes", EditorStyles.boldLabel);

            for (int i = 0; i < _mAttributes.arraySize; i++)
            {
                var attribute = _mAttributes.GetArrayElementAtIndex(i);
                var type = attribute.FindPropertyRelative("type");
                var value = attribute.FindPropertyRelative("value");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(type, GUIContent.none);
                EditorGUILayout.PropertyField(value, GUIContent.none);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    _mAttributes.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Attribute"))
            {
                _mAttributes.arraySize++;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
} 