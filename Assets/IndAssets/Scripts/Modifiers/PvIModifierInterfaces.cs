using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Modifiers
{
    public interface IDependencyInjectionSupportedModifier
    {
        public void AddModifier(AttributeModifier modifier);

        public void RegisterModifier(UnityAction<IAttributeOwner, IAttributeModifierContainer> modifierAction);

        public void UnregisterModifier(UnityAction<IAttributeOwner, IAttributeModifierContainer> modifierAction);

        public void UnregisterAll();
        
        public int CalculateResult(IAttributeOwner owner, float inputValue);
    }

    public interface IFinalReceiveDamageModifier : IDependencyInjectionSupportedModifier
    {
        // Empty
    }

    public interface IFinalCalculateDamageModifier : IDependencyInjectionSupportedModifier
    {
        // Empty
    }
}