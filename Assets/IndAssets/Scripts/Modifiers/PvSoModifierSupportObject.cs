using System.Collections.Generic;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.Utilities.Runtime.Events;
using ProjectCI.Utilities.Runtime.Modifiers;
using ProjectCI.Utilities.Runtime.Modifiers.Concrete;
using UnityEngine;

namespace IndAssets.Scripts.Modifiers
{
    [StaticInjectableTarget]
    [CreateAssetMenu(fileName = "ModifierSupportObject", menuName = "ProjectCI Utilities/Modifiers/ModifierSupportObject")]
    public class PvSoModifierSupportObject : ScriptableObject
    {
        [Inject] private static readonly PvSoModifiersManager ModifiersManager;
        [SerializeField] private AttributeType targetAttribute;
        
        [SerializeField]
        private AttributeModifier modifierType;

        protected virtual AttributeModifier GetDetail(UnitAttributeContainer container) => modifierType;

        private readonly Dictionary<string, UnitAttributeContainer> _registeredUnitContainers = new();

        public void Apply(PvMnBattleGeneralUnit unit)
        {
            var hasEverRegistered = _registeredUnitContainers.Count > 0;
            if (!_registeredUnitContainers.TryAdd(unit.EventIdentifier, unit.RuntimeAttributes) || hasEverRegistered)
            {
                return;
            }

            ModifiersManager.RegisterModifier(targetAttribute, ModifyAttribute);
        }

        public void UnApply(PvMnBattleGeneralUnit unit)
        {
            _registeredUnitContainers.Remove(unit.EventIdentifier);

            if (_registeredUnitContainers.Count <= 0)
            {
                ModifiersManager.UnregisterModifier(targetAttribute, ModifyAttribute);
            }
        }
    
        private void ModifyAttribute(IEventOwner attributeOwner, IAttributeModifierContainer container)
        {
            var key = attributeOwner.EventIdentifier;
            if (_registeredUnitContainers.TryGetValue(key, out var unitContainer))
            {
                container.AddModifier(GetDetail(unitContainer));
            }
        }
    }
}