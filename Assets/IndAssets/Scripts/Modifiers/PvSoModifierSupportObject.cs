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

        private readonly HashSet<string> _registeredUnit = new();

        public void Apply(PvMnBattleGeneralUnit unit)
        {
            var hasEverRegistered = _registeredUnit.Count > 0;
            _registeredUnit.Add(unit.EventIdentifier);

            if (hasEverRegistered) return;
            ModifiersManager.RegisterModifier(targetAttribute, ModifyAttribute);

        }

        public void UnApply(PvMnBattleGeneralUnit unit)
        {
            _registeredUnit.Remove(unit.EventIdentifier);

            if (_registeredUnit.Count <= 0)
            {
                ModifiersManager.UnregisterModifier(targetAttribute, ModifyAttribute);
            }
        }
    
        private void ModifyAttribute(IEventOwner attributeOwner, IAttributeModifierContainer container)
        {
            if (_registeredUnit.Contains(attributeOwner.EventIdentifier))
            {
                container.AddModifier(modifierType);
            }
        }
    }
}