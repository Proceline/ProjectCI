using System.Collections.Generic;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.Utilities.Runtime.Events;
using ProjectCI.Utilities.Runtime.Modifiers.Concrete;
using UnityEngine;

namespace ProjectCI.TacticTool.Formula.Concrete
{
    [StaticInjectableTarget]
    public class FormulaAttributeContainer : UnitAttributeContainer
    {
        private readonly Dictionary<AttributeType, FormulaDefinition> _attributesFormulaMap = new();
        private readonly IEventOwner _eventOwner;
        
        [Inject]
        private static PvSoModifiersManager _modifiersManager;
        
        public FormulaAttributeContainer(FormulaCollection formulaCollection, IEventOwner eventOwner)
        {
            if (formulaCollection != null)
            {
                foreach (var formulaDefinition in formulaCollection.Formulas)
                {
                    SetFormulaAttribute(formulaDefinition);
                }
            }

            _eventOwner = eventOwner;
        }

        private void SetFormulaAttribute(FormulaDefinition formulaDefinition)
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
            if (GeneralAttributes.TryGetValue(type, out var basicValue))
            {
                return GetAttributeValue(type);
            }

            if (_attributesFormulaMap.TryGetValue(type, out var formulaDefinition))
            {
                float formulaBasicValue = FormulaCalculator.CalculateFormula(formulaDefinition, GetAttributeValue);
                return _modifiersManager.GetModifiedValue(_eventOwner, type, formulaBasicValue);
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
                float basicValue = base.GetAttributeValue(type);
                return _modifiersManager.GetModifiedValue(_eventOwner, type, basicValue);
            }
            else
            {
                return GetAdvancedAttributeValue(type);
            }
        }
    }
} 