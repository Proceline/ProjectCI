using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Pools;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Commands.Concrete
{
    /// <summary>
    /// The result of a command execution, can be sent to frontend for animation.
    /// </summary>
    public abstract class PvConcreteCommand : CommandResult
    {
        [NonSerialized]
        protected UnitAbilityCore RuntimeAbility;

        [NonSerialized] 
        protected LevelCellBase TargetCell;

        [NonSerialized]
        protected GridObject TargetObject;

        public override void AddReaction(UnitAbilityCore ability, Queue<Action<GridPawnUnit>> reactions)
        {
            RuntimeAbility = ability;
            TargetCell = TacticBattleManager.GetGrid()[TargetCellIndex];
            TargetObject = TargetCell.GetObjectOnCell();
        }

        protected void ShowEffectOnTarget(Vector3 position)
        {
            if (RuntimeAbility.GetTargetParticles().Count == 0) return;

            foreach (var effectPrefab in RuntimeAbility.GetTargetParticles())
            {
                var hitEffect = MnObjectPool.Instance.Get(effectPrefab);
                hitEffect.transform.position = position + Vector3.up * 2;
            }
        }
    }
} 