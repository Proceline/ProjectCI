using ProjectCI.Utilities.Runtime.Events;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Modifiers
{
    public interface IDependencyInjectionSupportedModifier
    {
        public void AddModifier(AttributeModifier modifier);

        public void RegisterModifier(UnityAction<IEventOwner, IAttributeModifierContainer> modifierAction);

        public void UnregisterModifier(UnityAction<IEventOwner, IAttributeModifierContainer> modifierAction);

        public void UnregisterAll();
        
        public int CalculateResult(IEventOwner owner, float inputValue);
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