using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using ProjectCI.CoreSystem.Runtime.Commands;
using System;
using IndAssets.Scripts.Commands;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public partial class FeLiteGameRules
    {
        private readonly List<CombatingQueryContext> _singleCombatQueryAlloc =
            new(1) { new CombatingQueryContext { QueryType = CombatingQueryType.None } };

        /// <summary>
        /// This function applied after you get all results from logic level, after HandleAbilityCombatingLogic
        /// </summary>
        /// <param name="commandResults"></param>
        /// <exception cref="NullReferenceException"></exception>
        private async void HandleCommandResultsCoroutine(Queue<CommandResult> commandResults)
        {
            var bSequenceHead = true;
            CommandResult lastResult = null;
            LevelCellBase lastAimCell = null;
            GridPawnUnit owner = null;
            var commandsToApply = new Queue<CommandResult>();

            while (commandResults.TryDequeue(out var commandResult))
            {
                if (!_abilityIdToAbilityHash.TryGetValue(commandResult.AbilityId, out var ability))
                {
                    continue;
                }

                var isNewSequence = !bSequenceHead && commandResult.ResultId != lastResult.ResultId;

                // If enter new sequence, then APPLY all reactions
                if (isNewSequence)
                {
                    await ApplyAnimationProcess(ability, owner, lastAimCell, commandsToApply);
                    bSequenceHead = true;
                }

                if (bSequenceHead)
                {
                    bSequenceHead = false;
                    commandsToApply.Clear();
                    lastResult = commandResult;
                    try
                    {
                        lastAimCell = TacticBattleManager.GetGrid()[commandResult.TargetCellIndex];
                        owner = _unitIdToBattleUnitHash[commandResult.OwnerId];
                    }
                    catch
                    {
                        throw new NullReferenceException();
                    }
                }

                commandsToApply.Enqueue(commandResult);

                // If last result founded, directly apply the process
                if (commandResults.Count != 0)
                {
                    continue;
                }

                await ApplyAnimationProcess(ability, owner, lastAimCell, commandsToApply);
                break;
            }

            raiserTurnAnimationEndEvent.Raise();
            ClearStateAndDeselectUnitCombo();
        }

        /// <summary>
        /// 开始进行对峙,主要攻击角色将会对目标角色发起Ability,需要取到主要角色的CurrentAbility以及可能出现目标角色的反击EquippedAbility
        /// </summary>
        /// <param name="abilityOwner">发起进攻/辅助/行动的角色</param>
        /// <param name="targetUnit">被标记的目标角色</param>
        /// <param name="results"></param>
        /// <returns></returns>
        private void HandleAbilityCombatingLogic(PvMnBattleGeneralUnit abilityOwner, PvMnBattleGeneralUnit targetUnit,
            ref Queue<CommandResult> results)
        {
            PvSoUnitAbility ability = CurrentAbility;
            PvSoUnitAbility targetAbility = targetUnit.EquippedAbility;

            if (!ability || !targetAbility)
            {
                throw new NullReferenceException("ERROR: One of these two pawns missing ability!");
            }

            int abilitySpeed = abilityOwner.RuntimeAttributes.GetAttributeValue(abilitySpeedAttributeType);
            int targetAbilitySpeed = targetUnit.RuntimeAttributes.GetAttributeValue(abilitySpeedAttributeType);

            var combatContextList = ability.OnCombatingQueryListCreated(abilityOwner, targetUnit,
                abilitySpeed >= targetAbilitySpeed + followAttackSpeedThreshold,
                targetAbilitySpeed >= abilitySpeed + followAttackSpeedThreshold);
            RaiserOnCombatingListCreatedEvent.Raise(abilityOwner, targetUnit, combatContextList);

            foreach (CombatingQueryContext combatActionContext in combatContextList)
            {
                _singleCombatQueryAlloc[0] = combatActionContext;
                
                var combatAbility = combatActionContext.IsCounter ? targetAbility : ability;
                var caster = combatActionContext.IsCounter ? targetUnit : abilityOwner;
                var victim = combatActionContext.IsCounter ? abilityOwner : targetUnit;
                if (!combatAbility) continue;
                RaiserOnCombatingQueryStartEvent.Raise(abilityOwner, targetUnit, _singleCombatQueryAlloc);
                HandleAbilityParam(combatAbility, caster, victim, results);
                RaiserOnCombatingQueryEndEvent.Raise(abilityOwner, targetUnit, _singleCombatQueryAlloc);
            }
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
        public static void HandleAbilityParam(UnitAbilityCore inAbility, GridPawnUnit caster, GridPawnUnit target,
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
        
        public async Awaitable ApplyAnimationProcess(PvSoUnitAbility ability, GridPawnUnit casterUnit,
            LevelCellBase target, Queue<CommandResult> commands)
        {
            if (ability.GetShape())
            {
                casterUnit.LookAtCell(target);

                await UnitAbilityCoreExtensions.WaitUntilLockReleased(casterUnit);
                var executedTime = await ability.WaitUntilProjectileFinished(casterUnit, target);

                // TODO: Handle Audio
                // AudioPlayData audioData = new AudioPlayData(audioOnExecute);
                // AudioHandler.PlayAudio(audioData, casterUnit.gameObject.transform.position);

                while (commands.TryDequeue(out var toDoCommand))
                {
                    toDoCommand.ApplyResultOnVisual(_unitIdToBattleUnitHash, _abilityIdToAbilityHash);
                }

                var abilityAnimation = ability.abilityAnimation;
                if (abilityAnimation)
                {
                    var timeRemaining = abilityAnimation.GetAnimationLength() - executedTime;
                    timeRemaining = Mathf.Max(0, timeRemaining);

                    await Awaitable.WaitForSecondsAsync(timeRemaining);
                }

                // TODO: Need a end of lock
                // TacticBattleManager.RemoveActionBeingPerformed();
            }
        }
    }
}