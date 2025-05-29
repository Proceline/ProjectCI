using System.Collections.Generic;
using System;
using ProjectCI.CoreSystem.Runtime.Attributes;
using UnityEngine;

namespace ProjectCI.TacticTool
{
    public class FormulaCalculator
    {
        public static float CalculateFormula(FormulaDefinition formula, IDictionary<AttributeType, int> attributeValues)
        {
            if (formula == null || formula.FormulaNodes.Count == 0)
                return 0f;

            var nodes = formula.FormulaNodes;
            var stack = new Stack<float>();

            foreach (var node in nodes)
            {
                switch (node.Type)
                {
                    case FormulaNode.NodeType.Attribute:
                        if (attributeValues.TryGetValue(node.AttributeType, out int value))
                        {
                            stack.Push((float)value);
                        }
                        else
                        {
                            stack.Push(0f); // Default value if attribute not found
                        }
                        break;

                    case FormulaNode.NodeType.Constant:
                        stack.Push(node.ConstantValue);
                        break;

                    case FormulaNode.NodeType.Operator:
                        if (stack.Count < 2)
                            throw new InvalidOperationException("Invalid formula: not enough operands for operator");

                        float b = stack.Pop();
                        float a = stack.Pop();
                        float result = Mathf.Floor(CalculateOperation(a, b, node.OperatorSymbol));
                        stack.Push(result);
                        break;
                }
            }

            if (stack.Count != 1)
                throw new InvalidOperationException("Invalid formula: incorrect number of operands");

            return stack.Pop();
        }

        private static float CalculateOperation(float a, float b, string operatorSymbol)
        {
            return operatorSymbol switch
            {
                "+" => a + b,
                "-" => a - b,
                "*" => a * b,
                "/" => b != 0 ? a / b : 0f,
                _ => throw new ArgumentException($"Unknown operator: {operatorSymbol}")
            };
        }
    }
} 