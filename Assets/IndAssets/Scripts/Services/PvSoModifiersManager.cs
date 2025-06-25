using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;

namespace ProjectCI.Utilities.Runtime.Modifiers.Concrete
{
    [Serializable]
    public struct AttributeModifierPair
    {
        public AttributeType attributeType;
        public SoNumericModifier modifier;
    }
    
    [CreateAssetMenu(fileName = "PvSoModifiersManager", menuName = "ProjectCI Utilities/Modifiers/PvSoModifiersManager")]
    public class PvSoModifiersManager : ScriptableObject, IService
    {
        [SerializeField] private List<AttributeModifierPair> numericModifiers;

        private readonly IDictionary<AttributeType, SoNumericModifier> _modifiersDic =
            new Dictionary<AttributeType, SoNumericModifier>();

        private static readonly ServiceLocator<PvSoModifiersManager> ModifierService = new();

        public float GetModifiedValuePrecisely(IEventOwner owner, AttributeType attributeType, float inputValue)
        {
            if (_modifiersDic.TryGetValue(attributeType, out SoNumericModifier modifier))
            {
                return modifier.CalculatePreciseResult(owner, inputValue);
            }

            return inputValue;
        }

        public int GetModifiedValue(IEventOwner owner, AttributeType attributeType, float inputValue)
        {
            if (_modifiersDic.TryGetValue(attributeType, out SoNumericModifier modifier))
            {
                return modifier.CalculateResult(owner, inputValue);
            }

            return Mathf.FloorToInt(inputValue);
        }
        
        public void Dispose()
        {
            foreach (var attributePair in numericModifiers)
            {
                attributePair.modifier.Reset();
                attributePair.modifier.UnregisterAll();
            }
        }

        public void Initialize()
        {
            foreach (var attributePair in numericModifiers)
            {
                _modifiersDic.Add(attributePair.attributeType, attributePair.modifier);
            }
        }

        public void Cleanup()
        {
            Dispose();
        }
    }
}