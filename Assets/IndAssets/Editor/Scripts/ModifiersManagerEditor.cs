using UnityEngine;
using UnityEditor;
using ProjectCI.CoreSystem.Runtime.Attributes;
using System.Linq;
using ProjectCI.Utilities.Runtime.Modifiers.Concrete;

namespace ProjectCI.Utilities.Editor
{
    [CustomEditor(typeof(PvSoModifiersManager))]
    public class ModifiersManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty _attributeModifiersProperty;
        private int _selectedAttributeTypeIndex = 0;
        
        private AttributeTypeDefinition _attributeTypeDefinition;
        private string[] _attributeTypeNames;

        private void OnEnable()
        {
            _attributeModifiersProperty = serializedObject.FindProperty("numericModifiers");
            LoadAttributeTypeDefinition();
        }

        private void LoadAttributeTypeDefinition()
        {
            string[] guids = AssetDatabase.FindAssets("t:AttributeTypeDefinition");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _attributeTypeDefinition = AssetDatabase.LoadAssetAtPath<AttributeTypeDefinition>(path);
                if (_attributeTypeDefinition != null)
                {
                    _attributeTypeNames = _attributeTypeDefinition.AttributeTypeNames.ToArray();
                }
            }
            else
            {
                _attributeTypeNames = new string[] { "None" };
            }
        }
        
        private void DrawAttributesArray()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Draw each element in one line, fill the box
            if (_attributeModifiersProperty != null)
            {
                for (int i = 0; i < _attributeModifiersProperty.arraySize; i++)
                {
                    var element = _attributeModifiersProperty.GetArrayElementAtIndex(i);
                    var attrTypeProp = element.FindPropertyRelative(nameof(AttributeModifierPair.attributeType));
                    var modifierProp = element.FindPropertyRelative(nameof(AttributeModifierPair.modifier));

                    // 获取类型索引
                    int typeIndex = -1;
                    if (attrTypeProp != null)
                    {
                        var typeValueProp = attrTypeProp.FindPropertyRelative("value");
                        if (typeValueProp != null && typeValueProp.propertyType == SerializedPropertyType.Integer)
                        {
                            typeIndex = typeValueProp.intValue;
                        }
                    }

                    EditorGUILayout.BeginHorizontal();
                    // Attribute type display (read-only, auto width)
                    string typeName = (typeIndex >= 0 && typeIndex < _attributeTypeNames.Length)
                        ? _attributeTypeNames[typeIndex]
                        : "Unknown";
                    EditorGUILayout.LabelField(typeName, GUILayout.ExpandWidth(true));
                    // Value field (editable, auto width)
                    if (modifierProp != null)
                    {
                        EditorGUILayout.PropertyField(modifierProp, GUIContent.none);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space(10);

            // Add/Remove buttons with attribute type selector
            EditorGUILayout.BeginHorizontal();
            _selectedAttributeTypeIndex = EditorGUILayout.Popup(_selectedAttributeTypeIndex, _attributeTypeNames, GUILayout.Width(150));
            
            if (GUILayout.Button("Add New Attribute"))
            {
                AddNewAttribute();
            }
            if (GUILayout.Button("Remove Selected Attribute"))
            {
                RemoveSelectedAttribute();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
        
        private void AddNewAttribute()
        {
            _attributeModifiersProperty.arraySize++;
            var newElement =
                _attributeModifiersProperty.GetArrayElementAtIndex(_attributeModifiersProperty.arraySize - 1);
            
            // Initialize new attribute with selected type
            var attrTypeProp = newElement.FindPropertyRelative(nameof(AttributeModifierPair.attributeType));
            var modifierProp = newElement.FindPropertyRelative(nameof(AttributeModifierPair.modifier));
            
            if (attrTypeProp != null)
            {
                var typeValueProp = attrTypeProp.FindPropertyRelative("value");
                if (typeValueProp != null && typeValueProp.propertyType == SerializedPropertyType.Integer)
                    typeValueProp.intValue = _selectedAttributeTypeIndex;
            }
            if (modifierProp != null) modifierProp.objectReferenceValue = null;
            serializedObject.ApplyModifiedProperties();
        }
        
        private void RemoveSelectedAttribute()
        {
            for (int i = 0; i < _attributeModifiersProperty.arraySize; i++)
            {
                var element = _attributeModifiersProperty.GetArrayElementAtIndex(i);
                var attrTypeProp = element.FindPropertyRelative(nameof(AttributeModifierPair.attributeType));
                int typeIndex = -1;
                if (attrTypeProp != null)
                {
                    var typeValueProp = attrTypeProp.FindPropertyRelative("value");
                    if (typeValueProp != null && typeValueProp.propertyType == SerializedPropertyType.Integer)
                    {
                        typeIndex = typeValueProp.intValue;
                    }
                }
                if (typeIndex == _selectedAttributeTypeIndex)
                {
                    _attributeModifiersProperty.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    break; // 只删第一个
                }
            }
            serializedObject.Update();
        }

        public override void OnInspectorGUI()
        {
            // Draw our custom attributes array UI
            DrawAttributesArray();

            serializedObject.ApplyModifiedProperties();
        }
    }
} 