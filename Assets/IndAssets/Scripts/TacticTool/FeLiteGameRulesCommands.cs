using IndAssets.Scripts.Commands;
using IndAssets.Scripts.Events;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Commands.Concrete;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.TacticTool.Formula.Concrete;
using ProjectCI.Utilities.Runtime.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public partial class FeLiteGameRules
    {
        private readonly List<CombatingQueryContext> _singleCombatQueryAlloc =
            new(1) { new CombatingQueryContext { QueryType = CombatingQueryType.None } };

        private FormulaCollection _formulaCollectionInstance;
        private FormulaCollection FormulaCollectionInstance
        {
            get
            {
                if (!_formulaCollectionInstance)
                {
                    var service = new ServiceLocator<FormulaCollection>();
                    _formulaCollectionInstance = service.Service;
                }
                return _formulaCollectionInstance;
            }
        }

        /// <summary>
        /// This function applied after you get all results from logic level, after HandleAbilityCombatingLogic
        /// </summary>
        /// <param name="commandResults"></param>
        /// <exception cref="NullReferenceException"></exception>
        private async void HandleCommandResultsCoroutine(Queue<CommandResult> commandResults)
        {
            var commandsToApply = new Queue<CommandResult>();
            var currentDieCommands = new List<CommandResult>();

            while (commandResults.TryDequeue(out var commandResult))
            {
                commandsToApply.Enqueue(commandResult);
                var requireApply = commandResults.Count == 0;
                if (!requireApply && commandResults.TryPeek(out var nextResult))
                {
                    if (nextResult is PvDieCommand)
                    {
                        currentDieCommands.Add(nextResult);
                        commandResults.Dequeue();
                        if (!commandResults.TryPeek(out nextResult))
                        {
                            requireApply = true;
                        }
                    }

                    if (!requireApply && nextResult.ResultId != commandResult.ResultId)
                    {
                        requireApply = true;
                    }
                }

                if (requireApply)
                {
                    var ability = _abilityIdToAbilityHash.ContainsKey(commandResult.AbilityId)
                        ? _abilityIdToAbilityHash[commandResult.AbilityId] : null;
                    await ApplyAnimationProcess(ability, 
                        _unitIdToBattleUnitHash[commandResult.OwnerId], 
                        TacticBattleManager.GetGrid()[commandResult.TargetCellIndex], 
                        commandsToApply);

                    if (currentDieCommands.Count > 0)
                    {
                        await Awaitable.WaitForSecondsAsync(0.25f);
                        foreach (var dieCommand in currentDieCommands)
                        {
                            dieCommand.ApplyResultOnVisual(_unitIdToBattleUnitHash, _abilityIdToAbilityHash);
                        }
                        currentDieCommands.Clear();
                        await Awaitable.WaitForSecondsAsync(0.25f);
                    }

                    commandsToApply.Clear();
                }
            }

            raiserTurnLockerEvent.Raise(false);
            ClearStateAndDeselectUnitCombo();
        }

        /// <summary>
        /// Use ability to attack/support a target only in logic level, 
        /// this function will not apply any visual effect, 
        /// it will only calculate the combat result and return a list of CommandResult to tell the view level what to do.
        /// </summary>
        /// <param name="abilityOwner">Who will use this ability</param>
        /// <param name="targetUnit">Who will receive the result</param>
        /// <param name="results">Results of this behaviour</param>
        /// <returns></returns>
        private void HandleAbilityCombatingLogic(PvSoUnitAbility ability, PvMnBattleGeneralUnit abilityOwner, PvMnBattleGeneralUnit targetUnit,
            ref Queue<CommandResult> results)
        {
            var abilitySpeed = abilityOwner.RuntimeAttributes.GetAttributeValue(FormulaCollectionInstance.AttackSpeedType);
            var targetAbilitySpeed = targetUnit.RuntimeAttributes.GetAttributeValue(FormulaCollectionInstance.AttackSpeedType);

            var speedDifference = FormulaCollectionInstance.AttackSpeedDifference;

            var combatContextList = ability.OnCombatingQueryListCreated(abilityOwner, targetUnit,
                abilitySpeed >= targetAbilitySpeed + speedDifference,
                targetAbilitySpeed >= abilitySpeed + speedDifference);
            RaiserOnCombatingListCreatedEvent.Raise(abilityOwner, targetUnit, combatContextList);

            foreach (CombatingQueryContext combatActionContext in combatContextList)
            {
                _singleCombatQueryAlloc[0] = combatActionContext;

                PvSoUnitAbility combatAbility = null;

                var queryType = combatActionContext.QueryType;
                switch (queryType)
                {
                    case CombatingQueryType.FirstAttempt:
                        combatAbility = combatActionContext.IsCounter ? targetUnit.CounterAbility : ability;
                        break;
                    case CombatingQueryType.AutoFollowUp:
                        combatAbility = combatActionContext.IsCounter ? targetUnit.FollowUpAbility : abilityOwner.FollowUpAbility;
                        break;
                    case CombatingQueryType.ExtraFollowUp:
                    case CombatingQueryType.ReplacedFollowUp:
                        combatAbility = combatActionContext.IsCounter ? targetUnit.FollowUpAbility : abilityOwner.FollowUpAbility;
                        break;
                }

                var caster = combatActionContext.IsCounter ? targetUnit : abilityOwner;
                var victim = combatActionContext.IsCounter ? abilityOwner : targetUnit;
                if (!combatAbility && queryType != CombatingQueryType.Additional)
                {
                    continue;
                }

                RaiserOnCombatingQueryStartEvent.Raise(abilityOwner, targetUnit, _singleCombatQueryAlloc);
                
                if (queryType == CombatingQueryType.ReplacedFollowUp || queryType == CombatingQueryType.Additional)
                {
                    caster.ApplyAdjustedAction(combatActionContext, victim, results);
                }
                else
                {
                    combatAbility.HandleAbilityParam(caster, victim, results);
                }

                RaiserOnCombatingQueryEndEvent.Raise(abilityOwner, targetUnit, _singleCombatQueryAlloc);
            }
        }

        private async Awaitable ApplyAnimationProcess(PvSoUnitAbility ability, GridPawnUnit casterUnit,
            LevelCellBase target, Queue<CommandResult> commands)
        {
            if (casterUnit.GetCell() != target)
            {
                casterUnit.LookAtCell(target);
            }

            await UnitAbilityCoreExtensions.WaitUntilLockReleased(casterUnit);

            var executedTime = ability ? await ability.WaitUntilProjectileFinished(casterUnit, target) : 0;

            // TODO: Handle Audio
            // AudioPlayData audioData = new AudioPlayData(audioOnExecute);
            // AudioHandler.PlayAudio(audioData, casterUnit.gameObject.transform.position);

            while (commands.TryDequeue(out var toDoCommand))
            {
                toDoCommand.ApplyResultOnVisual(_unitIdToBattleUnitHash, _abilityIdToAbilityHash);
            }

            if (!ability)
            {
                await Awaitable.WaitForSecondsAsync(0.25f);
                return;
            }

            var abilityAnimation = ability.abilityAnimation;
            if (abilityAnimation)
            {
                var timeRemaining = abilityAnimation.GetAnimationLength() - executedTime;
                timeRemaining = Mathf.Max(0, timeRemaining);

                await Awaitable.WaitForSecondsAsync(timeRemaining);
            }
        }
    }
}