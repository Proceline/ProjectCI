using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Attributes;
using UnityEngine;

namespace ProjectCI.TacticTool.Formula.Concrete
{
    // Inherit all attribute logic from UnitAttributeContainer
    public class FormulaAttributeContainer : UnitAttributeContainer
    {
        private Dictionary<AttributeType, FormulaDefinition> _attributesFormulaMap = new();

        public void SetFormulaAttribute(FormulaDefinition formulaDefinition)
        {
            if (!_attributesFormulaMap.ContainsKey(formulaDefinition.TargetAttribute))
            {
                _attributesFormulaMap.Add(formulaDefinition.TargetAttribute, formulaDefinition);
            }
            else
            {
                Debug.LogError($"FormulaDefinition for attribute {formulaDefinition.TargetAttribute} already exists");
            }
        }

        public int GetFormulaResult(AttributeType attributeType)
        {
            if (_attributesFormulaMap.TryGetValue(attributeType, out var formulaDefinition))
            {
                
            }
            return 0;
        }

    }
} 