using UnityEditor;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Weapons;
using UnityEngine;

namespace ProjectCI.TacticTool.Editor
{
    [CustomEditor(typeof(SoWeaponData))]
    public class SoWeaponDataEditor : UnityEditor.Editor
    {
        private SerializedProperty m_WeaponNumberIdentifier;
        private SerializedProperty m_WeaponName;
        private SerializedProperty m_Description;
        private SerializedProperty m_Attributes;

        private void OnEnable()
        {
            m_WeaponNumberIdentifier = serializedObject.FindProperty("m_WeaponNumberIdentifier");
            m_WeaponName = serializedObject.FindProperty("weaponName");
            m_Description = serializedObject.FindProperty("description");
            m_Attributes = serializedObject.FindProperty("attributes");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_WeaponNumberIdentifier);
            EditorGUILayout.PropertyField(m_WeaponName);
            EditorGUILayout.PropertyField(m_Description);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Attributes", EditorStyles.boldLabel);

            for (int i = 0; i < m_Attributes.arraySize; i++)
            {
                var attribute = m_Attributes.GetArrayElementAtIndex(i);
                var type = attribute.FindPropertyRelative("type");
                var value = attribute.FindPropertyRelative("value");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(type, GUIContent.none);
                EditorGUILayout.PropertyField(value, GUIContent.none);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    m_Attributes.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Attribute"))
            {
                m_Attributes.arraySize++;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
} 