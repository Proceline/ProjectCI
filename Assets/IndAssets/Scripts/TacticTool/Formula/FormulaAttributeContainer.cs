using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.Utilities.Runtime.Events;
using ProjectCI.Utilities.Runtime.Modifiers.Concrete;
using UnityEngine;

namespace ProjectCI.TacticTool.Formula.Concrete
{
    // Inherit all attribute logic from UnitAttributeContainer
    public class FormulaAttributeContainer : UnitAttributeContainer
    {
        private readonly FormulaAttributeDictionary _formulaAttributeDictionary;
        private readonly Dictionary<AttributeType, FormulaDefinition> _attributesFormulaMap = new();
        private readonly IEventOwner _eventOwner;
        
        [Inject, NonSerialized]
        private PvSoModifiersManager _modifiersManager;

        public FormulaAttributeContainer()
        {
            
        }
        
        public FormulaAttributeContainer(FormulaCollection formulaCollection, IEventOwner eventOwner)
        {
            DIConfiguration.InjectFromConfiguration(this);
            _formulaAttributeDictionary = new FormulaAttributeDictionary(this);
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
            if (_attributesFormulaMap.TryGetValue(type, out var formulaDefinition))
            {
                float formulaBasicValue = FormulaCalculator.CalculateFormula(formulaDefinition, _formulaAttributeDictionary);
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