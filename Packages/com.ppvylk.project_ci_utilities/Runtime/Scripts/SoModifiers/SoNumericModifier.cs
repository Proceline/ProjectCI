using System;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Modifiers
{
    [Serializable]
    public struct AttributeModifier
    {
        public int flatValue;
        public float percentAddValue;

        public void Reset()
        {
            flatValue = 0;
            percentAddValue = 0;
        }
    }
    
    public interface IAttributeModifierContainer
    {
        void AddModifier(AttributeModifier modifier);
        void Reset();
    }

    [CreateAssetMenu(fileName = "SoNumericModifier", menuName = "ProjectCI Utilities/Modifiers/SoNumericModifier")]
    public class SoNumericModifier : ScriptableObject, IAttributeModifierContainer
    {
        private const float MinPercent = -1f;
        private const float MaxPercent = 999f;
        private const float MinBaseValue = 0f;
        private const float MaxBaseValue = 99999999f;
        
        private AttributeModifier _internalModifier;

        [SerializeField] 
        private UnityEvent<IEventOwner, IAttributeModifierContainer> collectedModifiers = new();
        
        public void AddModifier(AttributeModifier modifier)
        {
            _internalModifier.flatValue += modifier.flatValue;
            _internalModifier.percentAddValue += modifier.percentAddValue;
        }

        public void Reset()
        {
            _internalModifier.Reset();
        }

        public void RegisterModifier(UnityAction<IEventOwner, IAttributeModifierContainer> modifierAction)
        {
            collectedModifiers.AddListener(modifierAction);
        }

        public void UnregisterModifier(UnityAction<IEventOwner, IAttributeModifierContainer> modifierAction)
        {
            collectedModifiers.RemoveListener(modifierAction);
        }

        public void UnregisterAll()
        {
            collectedModifiers.RemoveAllListeners();
        }

        public float CalculatePreciseResult(IEventOwner owner, float inputValue)
        {
            Reset();
            collectedModifiers.Invoke(owner, this);

            float flatFinalValue = Mathf.Clamp(inputValue + _internalModifier.flatValue, MinBaseValue, MaxBaseValue);
            float percentAddValue = Mathf.Clamp(_internalModifier.percentAddValue, MinPercent, MaxPercent);
            float finalValue = Mathf.Floor(flatFinalValue * (1f + percentAddValue));
            
            return finalValue;
        }

        public int CalculateResult(IEventOwner owner, float inputValue) =>
            Mathf.FloorToInt(CalculatePreciseResult(owner, inputValue));
    }
}