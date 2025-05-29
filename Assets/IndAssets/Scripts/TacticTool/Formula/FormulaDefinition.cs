using UnityEngine;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Attributes;

namespace ProjectCI.TacticTool
{
    [CreateAssetMenu(fileName = "FormulaDefinition", menuName = "ProjectCI/Attributes/Create FormulaDefinition")]
    public class FormulaDefinition : ScriptableObject
    {
        [SerializeField]
        private AttributeType targetAttribute;  // Target attribute type (e.g., A)
        public AttributeType TargetAttribute => targetAttribute;

        [SerializeField]
        private List<FormulaNode> formulaNodes = new List<FormulaNode>();  // List of formula nodes
        public IReadOnlyList<FormulaNode> FormulaNodes => formulaNodes;

        // Add a new node to the formula
        public void AddNode(FormulaNode node)
        {
            formulaNodes.Add(node);
        }

        // Remove a node at the specified index
        public void RemoveNodeAt(int index)
        {
            if (index >= 0 && index < formulaNodes.Count)
            {
                formulaNodes.RemoveAt(index);
            }
        }

        // Clear all nodes
        public void ClearNodes()
        {
            formulaNodes.Clear();
        }

        // Get the formula as a string representation
        public string GetFormulaString()
        {
            string result = "";
            foreach (var node in formulaNodes)
            {
                switch (node.Type)
                {
                    case FormulaNode.NodeType.Attribute:
                        // TODO: Get attribute name from AttributeTypeDefinition
                        result += "[" + node.AttributeType.Value + "]";
                        break;
                    case FormulaNode.NodeType.Operator:
                        result += " " + node.OperatorSymbol + " ";
                        break;
                    case FormulaNode.NodeType.Constant:
                        result += node.ConstantValue.ToString();
                        break;
                }
            }
            return result;
        }
    }
} 