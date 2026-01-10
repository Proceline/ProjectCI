using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
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
    public sealed class PvSoPassivePerEneryRule : PvSoPassiveGlobal
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
            if (!(attributeOwner is GridPawnUnit gridPawnUnit))
            {
                return;
            }
            
            AIRadiusInfo radiusInfo = new AIRadiusInfo(gridPawnUnit.GetCell(), radius)
            {
                Caster = gridPawnUnit,
                bAllowBlocked = false,
                bStopAtBlockedCell = false,
                EffectedTeam = teamCondition
            };

            List<LevelCellBase> radCells = AStarAlgorithmUtils.GetRadius(radiusInfo);

            Debug.LogError($"Number of Teammates in Radius: {radCells.Count}!!!!");

            var numOfTeammates = radCells.Count;

            if (numOfTeammates == 0)
            {
                return;
            }
            else if (numOfTeammates == 1)
            {
                container.AddModifier(attributeModifier);
            }
        }

    }
}