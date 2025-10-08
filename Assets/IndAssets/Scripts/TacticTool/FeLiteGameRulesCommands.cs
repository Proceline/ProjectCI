using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using ProjectCI.CoreSystem.Runtime.Commands;
using System;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Abilities.Enums;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public partial class FeLiteGameRules
    {
        private readonly Queue<Action<GridPawnUnit>> _bufferedReacts = new();
        
        private async void HandleCommandResultsCoroutine(Queue<CommandResult> results)
        {
            var bSequenceHead = true;
            CommandResult lastResult = null;
            LevelCellBase lastAimCell = null;
            GridPawnUnit owner = null;
            _bufferedReacts.Clear();

            while (results.TryDequeue(out var result))
            {
                if (!_abilityIdToAbilityHash.TryGetValue(result.AbilityId, out var ability))
                {
                    continue;
                }

                var isNewSequence = !bSequenceHead && result.ResultId != lastResult.ResultId;
                
                // If enter new sequence, then APPLY all reactions
                if (isNewSequence)
                {
                    await UnitAbilityCoreExtensions.ApplyAnimationProcess(ability, owner, lastAimCell, _bufferedReacts);
                    bSequenceHead = true;
                }

                if (bSequenceHead)
                {
                    bSequenceHead = false;
                    _bufferedReacts.Clear();
                    lastResult = result;
                    try
                    {
                        lastAimCell = TacticBattleManager.GetGrid()[result.TargetCellIndex];
                        owner = _unitIdToBattleUnitHash[result.OwnerId];
                    }
                    catch
                    {
                        throw new NullReferenceException();
                    }
                }

                result.AddReaction(ability, _bufferedReacts);

                // If last result founded, directly apply the process
                if (results.Count != 0)
                {
                    continue;
                }

                await UnitAbilityCoreExtensions.ApplyAnimationProcess(ability, owner, lastAimCell, _bufferedReacts);
                break;
            }
            
            raiserTurnAnimationEndEvent.Raise();
            ClearStateAndDeselectUnit();
        }

        /// <summary>
        /// 开始进行对峙,主要攻击角色将会对目标角色发起Ability,需要取到主要角色的CurrentAbility以及可能出现目标角色的反击EquippedAbility
        /// </summary>
        /// <param name="abilityOwner">发起进攻/辅助/行动的角色</param>
        /// <param name="targetUnit">被标记的目标角色</param>
        /// <returns></returns>
        private Queue<CommandResult> HandleAbilityCombatingLogic(PvMnBattleGeneralUnit abilityOwner, PvMnBattleGeneralUnit targetUnit)
        {
            PvSoUnitAbility ability = CurrentAbility;
            PvSoUnitAbility targetAbility = targetUnit.EquippedAbility;

            if (!ability || !targetAbility)
            {
                throw new NullReferenceException("ERROR: One of these two pawns missing ability!");
            }

            List<LevelCellBase> targetAbilityCells = targetAbility.GetAbilityCells(targetUnit);
            bool bIsTargetAbilityAbleToCounter = targetAbilityCells.Count > 0 && targetAbilityCells.Contains(abilityOwner.GetCell());

            int abilitySpeed = abilityOwner.RuntimeAttributes.GetAttributeValue(abilitySpeedAttributeType);
            int targetAbilitySpeed = targetUnit.RuntimeAttributes.GetAttributeValue(abilitySpeedAttributeType);
            FollowUpCondition followUpCondition = FollowUpCondition.None;
            if (abilitySpeed >= targetAbilitySpeed + DoubleAttackSpeedThreshold && ability.IsFollowUpAllowed())
            {
                followUpCondition = FollowUpCondition.InitiativeFollowUp;
            }
            else if (targetAbilitySpeed >= abilitySpeed + DoubleAttackSpeedThreshold && targetAbility.IsFollowUpAllowed())
            {
                followUpCondition = FollowUpCondition.CounterFollowUp;
            }

            var results = new Queue<CommandResult>();
            List<CombatActionContext> combatActionContextList = ability.CreateCombatActionContextList(bIsTargetAbilityAbleToCounter, followUpCondition);

            foreach (CombatActionContext combatActionContext in combatActionContextList)
            {
                var combatAbility = combatActionContext.IsVictim ? targetAbility : ability;
                var caster = combatActionContext.IsVictim ? targetUnit : abilityOwner;
                var victim = combatActionContext.IsVictim ? abilityOwner : targetUnit;
                if (combatAbility)
                {
                    HandleAbilityParam(combatAbility, caster, victim, results);
                }
            }

            return results;
        }

        // TODO: Electric Chain-like skill
        /*
        private void HandleAbilityCombatingLogic(PvMnBattleGeneralUnit abilityOwner, LevelCellBase targetCell,
            List<CommandResult> results)
        {
            PvSoUnitAbility ability = CurrentAbility;

            if (!ability)
            {
                throw new NullReferenceException("ERROR: Owner missing ability!");
            }

            var targetUnit = targetCell.GetUnitOnCell();
            if (!targetUnit)
            {
                return;
            }

            HandleAbilityParam(ability, abilityOwner, targetUnit, ability, results);
        }
*/
        private void HandleAbilityParam(UnitAbilityCore inAbility, GridPawnUnit caster, GridPawnUnit target,
            Queue<CommandResult> results)
        {
            if (caster.IsDead())
            {
                return;
            }

            var resultId = Guid.NewGuid().ToString();
            foreach (AbilityParamBase param in inAbility.GetParameters())
            {
                param.Execute(resultId, inAbility, caster, target, results);
            }
        }
    }
}