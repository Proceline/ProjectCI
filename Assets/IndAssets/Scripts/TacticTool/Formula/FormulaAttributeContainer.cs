using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Attributes;
using UnityEngine;

namespace ProjectCI.TacticTool.Formula.Concrete
{
    // Inherit all attribute logic from UnitAttributeContainer
    public class FormulaAttributeContainer : UnitAttributeContainer
    {
        private readonly FormulaAttributeDictionary _formulaAttributeDictionary;
        private Dictionary<AttributeType, FormulaDefinition> _attributesFormulaMap = new();

        public FormulaAttributeContainer()
        {
            _formulaAttributeDictionary = new FormulaAttributeDictionary(this);
        }

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
        
        private int GetAdvancedAttributeValue(AttributeType type)
        {
            if (_attributesFormulaMap.TryGetValue(type, out var formulaDefinition))
            {
                return Mathf.FloorToInt(FormulaCalculator.CalculateFormula(formulaDefinition, _formulaAttributeDictionary));
            }
            return 0;
        }

        public bool IsRegistered(AttributeType type)
        {
            return GeneralAttributes.ContainsKey(type);
        }

        public override int GetAttributeValue(AttributeType type)
        {
            if (GeneralAttributes.ContainsKey(type))
            {
                return base.GetAttributeValue(type);
            }
            else
            {
                return GetAdvancedAttributeValue(type);
            }
        }
    }
} 