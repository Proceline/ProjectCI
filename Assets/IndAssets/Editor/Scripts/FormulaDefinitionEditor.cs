using UnityEngine;
using UnityEditor;
using ProjectCI.CoreSystem.Runtime.Attributes;
using System.Linq;
using System.Collections.Generic;

namespace ProjectCI.TacticTool.Editor
{
    [CustomEditor(typeof(FormulaDefinition))]
    public class FormulaDefinitionEditor : UnityEditor.Editor
    {
        private SerializedProperty targetAttributeProperty;
        private SerializedProperty formulaNodesProperty;
        private AttributeTypeDefinition attributeTypeDefinition;
        private string[] attributeTypeNames;
        private string newOperatorSymbol = "+";
        private float newConstantValue = 0f;
        private Dictionary<AttributeType, float> testValues = new Dictionary<AttributeType, float>();
        private Vector2 testAreaScrollPosition;

        private void OnEnable()
        {
            targetAttributeProperty = serializedObject.FindProperty("targetAttribute");
            formulaNodesProperty = serializedObject.FindProperty("formulaNodes");
            LoadAttributeTypeDefinition();
        }

        private void LoadAttributeTypeDefinition()
        {
            string[] guids = AssetDatabase.FindAssets("t:AttributeTypeDefinition");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                attributeTypeDefinition = AssetDatabase.LoadAssetAtPath<AttributeTypeDefinition>(path);
                if (attributeTypeDefinition != null)
                {
                    attributeTypeNames = attributeTypeDefinition.AttributeTypeNames.ToArray();
                }
            }
            else
            {
                attributeTypeNames = new string[] { "None" };
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Formula Definition", EditorStyles.boldLabel);

            // Draw target attribute selector
            EditorGUILayout.PropertyField(targetAttributeProperty, new GUIContent("Target Attribute"));

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Formula Nodes", EditorStyles.boldLabel);

            // Draw existing nodes
            for (int i = 0; i < formulaNodesProperty.arraySize; i++)
            {
                var nodeProperty = formulaNodesProperty.GetArrayElementAtIndex(i);
                DrawNodeProperty(nodeProperty, i);
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Add New Node", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Attribute"))
            {
                AddAttributeNode();
            }
            if (GUILayout.Button("Add Operator"))
            {
                AddOperatorNode();
            }
            if (GUILayout.Button("Add Constant"))
            {
                AddConstantNode();
            }
            EditorGUILayout.EndHorizontal();

            // Draw formula preview
            EditorGUILayout.Space(10);
            var formula = target as FormulaDefinition;
            if (formula != null)
            {
                EditorGUILayout.LabelField("Formula Preview:", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(GetInfixFormulaString(formula), MessageType.None);

                // Draw test area
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Test Formula", EditorStyles.boldLabel);
                testAreaScrollPosition = EditorGUILayout.BeginScrollView(testAreaScrollPosition, GUILayout.Height(200));
                var usedAttributes = new HashSet<AttributeType>();
                foreach (var node in formula.FormulaNodes)
                {
                    if (node.Type == FormulaNode.NodeType.Attribute)
                        usedAttributes.Add(node.AttributeType);
                }
                foreach (var attrType in usedAttributes)
                {
                    if (!testValues.ContainsKey(attrType))
                        testValues[attrType] = 0f;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Test Value for [{attrType.Value}]:", GUILayout.Width(150));
                    testValues[attrType] = EditorGUILayout.FloatField(testValues[attrType]);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
                try
                {
                    float result = FormulaCalculator.CalculateFormula(formula, testValues);
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Test Result:", EditorStyles.boldLabel);
                    EditorGUILayout.HelpBox($"Result = {result}", MessageType.Info);
                }
                catch (System.Exception e)
                {
                    EditorGUILayout.HelpBox($"Error calculating formula: {e.Message}", MessageType.Error);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawNodeProperty(SerializedProperty nodeProperty, int index)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            var typeProperty = nodeProperty.FindPropertyRelative("type");
            var type = (FormulaNode.NodeType)typeProperty.enumValueIndex;
            switch (type)
            {
                case FormulaNode.NodeType.Attribute:
                    var attributeTypeProperty = nodeProperty.FindPropertyRelative("attributeType");
                    EditorGUILayout.PropertyField(attributeTypeProperty, GUIContent.none);
                    break;
                case FormulaNode.NodeType.Operator:
                    var operatorProperty = nodeProperty.FindPropertyRelative("operatorSymbol");
                    EditorGUILayout.PropertyField(operatorProperty, GUIContent.none);
                    break;
                case FormulaNode.NodeType.Constant:
                    var constantProperty = nodeProperty.FindPropertyRelative("constantValue");
                    EditorGUILayout.PropertyField(constantProperty, GUIContent.none);
                    break;
            }
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                formulaNodesProperty.DeleteArrayElementAtIndex(index);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void AddAttributeNode()
        {
            formulaNodesProperty.arraySize++;
            var newElement = formulaNodesProperty.GetArrayElementAtIndex(formulaNodesProperty.arraySize - 1);
            var typeProperty = newElement.FindPropertyRelative("type");
            typeProperty.enumValueIndex = (int)FormulaNode.NodeType.Attribute;
        }
        private void AddOperatorNode()
        {
            formulaNodesProperty.arraySize++;
            var newElement = formulaNodesProperty.GetArrayElementAtIndex(formulaNodesProperty.arraySize - 1);
            var typeProperty = newElement.FindPropertyRelative("type");
            typeProperty.enumValueIndex = (int)FormulaNode.NodeType.Operator;
            var operatorProperty = newElement.FindPropertyRelative("operatorSymbol");
            operatorProperty.stringValue = newOperatorSymbol;
        }
        private void AddConstantNode()
        {
            formulaNodesProperty.arraySize++;
            var newElement = formulaNodesProperty.GetArrayElementAtIndex(formulaNodesProperty.arraySize - 1);
            var typeProperty = newElement.FindPropertyRelative("type");
            typeProperty.enumValueIndex = (int)FormulaNode.NodeType.Constant;
            var constantProperty = newElement.FindPropertyRelative("constantValue");
            constantProperty.floatValue = newConstantValue;
        }

        // Add this method to convert RPN to infix string
        private string GetInfixFormulaString(FormulaDefinition formula)
        {
            if (formula == null || formula.FormulaNodes == null)
                return string.Empty;
            var stack = new Stack<string>();
            foreach (var node in formula.FormulaNodes)
            {
                switch (node.Type)
                {
                    case FormulaNode.NodeType.Attribute:
                        string attrName = GetAttributeTypeName(node.AttributeType);
                        stack.Push(attrName);
                        break;
                    case FormulaNode.NodeType.Constant:
                        stack.Push(node.ConstantValue.ToString());
                        break;
                    case FormulaNode.NodeType.Operator:
                        if (stack.Count < 2) return "Invalid formula";
                        string b = stack.Pop();
                        string a = stack.Pop();
                        stack.Push($"({a}{node.OperatorSymbol}{b})");
                        break;
                }
            }
            return stack.Count == 1 ? stack.Pop() : "Invalid formula";
        }

        // Helper to get attribute name from definition
        private string GetAttributeTypeName(AttributeType type)
        {
            if (attributeTypeDefinition != null && type.Value >= 0 && type.Value < attributeTypeNames.Length)
                return attributeTypeNames[type.Value];
            return $"[{type.Value}]";
        }
    }
} 