using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;
using ProjectCI.Utilities.Runtime.Modifiers;
using ProjectCI.Utilities.Runtime.Modifiers.Concrete;
using UnityEngine;
using UnityEngine.Events;
using AttributeModifier = ProjectCI.Utilities.Runtime.Modifiers.AttributeModifier;

namespace ProjectCI.CoreSystem.Runtime.Passives
{
    
    [CreateAssetMenu(fileName = "SoPassiveCustomValueModifier", menuName = "Project CI Passives/CustomValueModifier", order = 1)]
    public class PvSoPassiveCustomValueModifier : PvSoPassiveBase
    {
        [Header("Modifier Details"), SerializeField] 
        private PvSoCustomValueModifier customValueModifier;
        
        [SerializeField] 
        private AttributeModifier attributeModifier;

        private readonly Dictionary<string, UnityAction<IEventOwner, IAttributeModifierContainer>>
            _loadedModifierActions = new();

        protected override void InstallPassiveInternally(GridPawnUnit unit)
        {
            Debug.Log($"Initialize Passive <{name}> to {unit.name}");
            if (!_loadedModifierActions.ContainsKey(unit.ID))
            {
                GridPawnUnit bufferedUnit = unit;
                UnityAction<IEventOwner, IAttributeModifierContainer> modifierAction = (eventOwner, container) =>
                    ModifyValueForOwner(bufferedUnit, eventOwner, container);
                _loadedModifierActions.Add(unit.ID, modifierAction);
                customValueModifier.RegisterModifier(modifierAction);
            }
            else
            {
                Debug.LogWarning($"{unit.name} already assigned this Passive {name}");
            }
        }

        protected override void DisposePassiveInternally(GridPawnUnit unit)
        {
            if (_loadedModifierActions.TryGetValue(unit.ID, out var modifierAction))
            {
                customValueModifier.UnregisterModifier(modifierAction);
                _loadedModifierActions.Remove(unit.ID);
            }
            else
            {
                Debug.LogWarning($"{unit.name} didn't assign any of this Passive {name}");
            }
        }

        private void ModifyValueForOwner(GridPawnUnit passiveOwner, IEventOwner triggerOwner, IAttributeModifierContainer container)
        {
            if (passiveOwner.ID == triggerOwner.EventIdentifier)
            {
                container.AddModifier(attributeModifier);
            }
        }
    }
}