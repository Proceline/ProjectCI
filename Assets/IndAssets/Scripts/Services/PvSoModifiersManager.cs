using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Modifiers.Concrete
{
    [Serializable]
    public struct AttributeModifierPair
    {
        public AttributeType attributeType;
        public SoNumericModifier modifier;
    }
    
    [CreateAssetMenu(fileName = "PvSoModifiersManager", menuName = "ProjectCI Utilities/Modifiers/PvSoModifiersManager")]
    public class PvSoModifiersManager : ScriptableObject
    {
        [SerializeField] private List<AttributeModifierPair> numericModifiers;

        private readonly IDictionary<AttributeType, SoNumericModifier> _modifiersDic =
            new Dictionary<AttributeType, SoNumericModifier>();

        public void RegisterModifier(AttributeType attributeType,
            UnityAction<IEventOwner, IAttributeModifierContainer> modifierAction)
        {
            if (_modifiersDic.TryGetValue(attributeType, out SoNumericModifier modifier))
            {
                modifier.RegisterModifier(modifierAction);
            }
            else
            {
                Debug.LogError($"Missing <AttributeType: {attributeType.Value}>");
            }
        }

        public void UnregisterModifier(AttributeType attributeType,
            UnityAction<IEventOwner, IAttributeModifierContainer> modifierAction)
        {
            if (_modifiersDic.TryGetValue(attributeType, out SoNumericModifier modifier))
            {
                modifier.UnregisterModifier(modifierAction);
            }
            else
            {
                Debug.LogError($"Missing <AttributeType: {attributeType.Value}>");
            }
        }
        
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

        private void Initialize()
        {
            foreach (var attributePair in numericModifiers)
            {
                _modifiersDic.Add(attributePair.attributeType, attributePair.modifier);
            }
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnGameLoadedBeforeScene()
        {
            var allManagers = Resources.LoadAll<PvSoModifiersManager>("");
            foreach (var soModifiersManager in allManagers)
            {
                soModifiersManager.Initialize();
            }
        }
    }
}