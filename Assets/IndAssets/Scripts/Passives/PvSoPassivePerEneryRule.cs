using System.Collections.Generic;
using IndAssets.Scripts.Units;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.TacticTool.Formula.Concrete;
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
    public sealed class PvSoPassivePerEneryRule : PvSoPassiveGlobal
    {
        [Header("Parameters"), SerializeField] 
        private AttributeType targetAttribute;

        [SerializeField]
        private BattleTeam teamCondition;

        [SerializeField]
        private int noPeopleEneryMultiplier = -10;

        [SerializeField]
        private int hasPeopleEneryMultiplier = 4;

        [Inject] private static readonly PvSoModifiersManager ModifiersManager;
        
        private static readonly ServiceLocator<FormulaCollection> FormulaService = new();
        internal static FormulaCollection FormulaColInstance => FormulaService.Service;

        
        private void Initialize()
        {
            ModifiersManager.RegisterModifier(targetAttribute, ModifyAttribute);
        }

        private void Dispose()
        {
            ModifiersManager.UnregisterModifier(targetAttribute, ModifyAttribute);
        }

        private void ModifyAttribute(IEventOwner attributeOwner, IAttributeModifierContainer container)
        {   
            var cell = TacticBattleManager.GetGrid()[attributeOwner.GridPosition];
            var ownerUnit = cell.GetUnitOnCell();

            if (!ownerUnit)
            {
                return;
            }

            var energyAttribute = FormulaColInstance.GetPersonalityAttribute(EPvPersonalityName.Energy);
            var energyLevel = ownerUnit.RuntimeAttributes.GetAttributeValue(energyAttribute);
            var radius = energyLevel >= 0? 2 : 1;

            AIRadiusInfo radiusInfo = new AIRadiusInfo(cell, radius)
            {
                Caster = ownerUnit,
                bAllowBlocked = false,
                bStopAtBlockedCell = false,
                EffectedTeam = teamCondition
            };

            List<LevelCellBase> radCells = AStarAlgorithmUtils.GetRadius(radiusInfo);

            Debug.LogError($"Number of Teammates in Radius: {radCells.Count}!!!!");
            var numOfTeammates = radCells.Count;

            var noPeopleModifier = new AttributeModifier
            {
                flatValue = energyLevel * noPeopleEneryMultiplier
            };

            var hasPeopleModifier = new AttributeModifier
            {
                flatValue = energyLevel * hasPeopleEneryMultiplier * numOfTeammates
            };

            if (numOfTeammates == 0)
            {
                container.AddModifier(noPeopleModifier);
                return;
            }
            else
            {
                container.AddModifier(hasPeopleModifier);
            }
        }

    }
}