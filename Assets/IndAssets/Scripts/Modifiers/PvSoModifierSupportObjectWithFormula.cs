using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.TacticTool;
using ProjectCI.Utilities.Runtime.Modifiers;
using UnityEngine;

namespace IndAssets.Scripts.Modifiers
{
    [CreateAssetMenu(fileName = "ModifierSupportObject_Formula", menuName = "ProjectCI Utilities/Modifiers/ModifierSupportObject (Formula)")]
    public class PvSoModifierSupportObjectWithFormula : PvSoModifierSupportObject
    {
        [SerializeField] private FormulaDefinition valueFormula;
        private readonly Dictionary<AttributeType, int> _preloadedAttributes = new();

        /// <summary>
        /// If TRUE, then, the result will never LESS than value, otherwise, the result will never MORE than value
        /// </summary>
        [SerializeField] private bool baseValueAsMinLimit = true;

        #region ExtensionOfAttributes
        [SerializeField] private AttributeType currentHitPointAttribute;
        #endregion

        protected override AttributeModifier GetDetail(UnitAttributeContainer container)
        {
            ResetAttributesHash(container);
            var result = FormulaCalculator.CalculateFormula(valueFormula, _preloadedAttributes);
            var initialBase = base.GetDetail(container);
            var limitValue = initialBase.flatValue;
            var calculatedValue = Mathf.FloorToInt(result);
            switch (baseValueAsMinLimit)
            {
                case true when calculatedValue > limitValue:
                case false when calculatedValue < limitValue:
                    initialBase.flatValue = calculatedValue;
                    break;
            }

            return initialBase;
        }

        private void ResetAttributesHash(UnitAttributeContainer container)
        {
            foreach (var node in valueFormula.FormulaNodes)
            {
                if (node.Type != FormulaNode.NodeType.Attribute)
                {
                    continue;
                }

                var attributeType = node.AttributeType;

                if (attributeType == currentHitPointAttribute)
                {
                    _preloadedAttributes[attributeType] = container.Health.CurrentValue;
                    continue;
                }
                
                var currentResultValue = container.GetAttributeValue(attributeType);
                _preloadedAttributes[attributeType] = currentResultValue;
            }
        }
    }
}