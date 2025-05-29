using UnityEngine;
using System;
using ProjectCI.CoreSystem.Runtime.Attributes;

namespace ProjectCI.TacticTool
{
    [Serializable]
    public class FormulaNode
    {
        public enum NodeType
        {
            Attribute,  // Reference to another attribute (e.g., B, C)
            Operator,   // Operator (e.g., +, -, *, /)
            Constant    // Constant value
        }
        
        [SerializeField]
        private NodeType type;
        public NodeType Type => type;

        [SerializeField]
        private AttributeType attributeType;  // Used when type is Attribute
        public AttributeType AttributeType => attributeType;

        [SerializeField]
        private string operatorSymbol;  // Used when type is Operator
        public string OperatorSymbol => operatorSymbol;

        [SerializeField]
        private float constantValue;  // Used when type is Constant
        public float ConstantValue => constantValue;

        // Constructor for Attribute type
        public static FormulaNode CreateAttributeNode(AttributeType attributeType)
        {
            return new FormulaNode
            {
                type = NodeType.Attribute,
                attributeType = attributeType
            };
        }

        // Constructor for Operator type
        public static FormulaNode CreateOperatorNode(string operatorSymbol)
        {
            return new FormulaNode
            {
                type = NodeType.Operator,
                operatorSymbol = operatorSymbol
            };
        }

        // Constructor for Constant type
        public static FormulaNode CreateConstantNode(float value)
        {
            return new FormulaNode
            {
                type = NodeType.Constant,
                constantValue = value
            };
        }
    }
} 