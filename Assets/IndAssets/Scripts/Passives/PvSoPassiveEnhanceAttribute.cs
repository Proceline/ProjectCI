using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Attributes;
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
    [StaticInjectableTarget]
    [CreateAssetMenu(fileName = "New EnhanceAttribute Passive", menuName = "ProjectCI Passives/EnhanceAttribute", order = 1)]
    public sealed class PvSoPassiveEnhanceAttribute : PvSoPassiveBase
    {
        [Header("Parameters"), SerializeField] 
        private AttributeType targetAttribute;

        [SerializeField] 
        private int radius = 1;

        [Header("Modifier Details"), SerializeField] 
        private AttributeModifier attributeModifier;

        [SerializeField]
        private BattleTeam teamCondition;

        [Inject] private static readonly PvSoModifiersManager ModifiersManager;

        private readonly Dictionary<string, UnityAction<IEventOwner, IAttributeModifierContainer>>
            _loadedModifierActions = new();

        protected override void InstallPassiveInternally(GridPawnUnit unit)
        {
            Debug.Log($"Initialize Passive <{name}> to {unit.name}");
            if (!_loadedModifierActions.ContainsKey(unit.ID))
            {
                GridPawnUnit bufferedUnit = unit;
                UnityAction<IEventOwner, IAttributeModifierContainer> modifierAction = (attributeOwner, container) =>
                    ModifyAttribute(bufferedUnit, attributeOwner, container);
                _loadedModifierActions.Add(unit.ID, modifierAction);
                ModifiersManager.RegisterModifier(targetAttribute, modifierAction);
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
                ModifiersManager.UnregisterModifier(targetAttribute, modifierAction);
                _loadedModifierActions.Remove(unit.ID);
            }
            else
            {
                Debug.LogWarning($"{unit.name} didn't assign any of this Passive {name}");
            }
        }

        private void ModifyAttribute(GridPawnUnit gridPawnUnit, IEventOwner attributeOwner, IAttributeModifierContainer container)
        {
            AIRadiusInfo radiusInfo = new AIRadiusInfo(gridPawnUnit.GetCell(), radius)
            {
                Caster = gridPawnUnit,
                bAllowBlocked = false,
                bStopAtBlockedCell = false,
                EffectedTeam = teamCondition
            };

            List<LevelCellBase> radCells = AStarAlgorithmUtils.GetRadius(radiusInfo);
            for (int i = 0; i < radCells.Count; i++)
            {
                if (radCells[i].GetIndex() == attributeOwner.GridPosition)
                {
                    container.AddModifier(attributeModifier);
                    break;
                }
            }
        }

        public void Test(IEventOwner attributeOwner, IAttributeModifierContainer container)
        {
            Debug.LogError($"Test Function Used: Calculate {targetAttribute}!!!!");
            container.AddModifier(attributeModifier);
        }
    }
}