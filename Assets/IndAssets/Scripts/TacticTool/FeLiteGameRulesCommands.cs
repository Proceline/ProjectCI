using IndAssets.Scripts.Abilities;
using IndAssets.Scripts.Commands;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.Abilities.Projectiles;
using ProjectCI.CoreSystem.Runtime.Animation;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.TacticTool.Formula.Concrete;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public partial class FeLiteGameRules
    {
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

        private readonly Dictionary<Type, Stack<CommandResult>> _commandsPool = new();

        public T GetCommand<T>() where T : CommandResult, new()
        {
            var type = typeof(T);
            if (!_commandsPool.ContainsKey(type))
            {
                _commandsPool[type] = new Stack<CommandResult>();
            }

            if (_commandsPool[type].Count > 0)
            {
                return _commandsPool[type].Pop() as T;
            }
            else
            {
                return new T();
            }
        }

        public void Release<T>(T command) where T : CommandResult
        {
            var type = typeof(T);
            if (!_commandsPool.ContainsKey(type))
            {
                _commandsPool[type] = new Stack<CommandResult>();
            }

            command.ClearCommand();
            _commandsPool[type].Push(command);
        }

        /// <summary>
        /// This function applied after you get all results from logic level
        /// </summary>
        /// <param name="queryItem"></param>
        /// <returns></returns>
        private async Awaitable ProcessVisualResults(PvAbilityQueryItem<PvMnBattleGeneralUnit> queryItem)
        {
            var commandResults = queryItem.Commands;
            if (commandResults.Count == 0)
            {
                return;
            }

            await ApplyAnimationProcess(queryItem.Ability, queryItem.holdingOwner, queryItem.targetUnit.GetCell(), commandResults);
        }

        private async Awaitable ApplyAnimationProcess(PvSoUnitAbility ability, GridPawnUnit casterUnit,
            LevelCellBase target, Queue<CommandResult> commands)
        {
            if (casterUnit.GetCell() != target)
            {
                casterUnit.LookAtCell(target);
            }

            await UnitAbilityCoreExtensions.WaitUntilLockReleased(casterUnit);

            var animationName = ability.AnimationName;
            var executedTime = await WaitUntilProjectileFinished(animationName, casterUnit, target, ability.ProjectilePrefab);

            // TODO: Handle Audio
            // AudioPlayData audioData = new AudioPlayData(audioOnExecute);
            // AudioHandler.PlayAudio(audioData, casterUnit.gameObject.transform.position);

            while (commands.TryDequeue(out var toDoCommand))
            {
                toDoCommand.ApplyResultOnVisual(ability, _unitIdToBattleUnitHash);
            }

            // If there is no ability, just wait for a while to make sure the visual result is applied after the logic result
            if (!ability)
            {
                await Awaitable.WaitForSecondsAsync(0.25f);
                return;
            }
;
            if (animationName != Animation.AnimationPvCustomName.DoNothing)
            {
                var timeRemaining = GetPresetAnimationLengthFunc.Raise(casterUnit.transform, animationName.ToString())
                    - executedTime;

                if (timeRemaining > 0)
                {
                    await Awaitable.WaitForSecondsAsync(timeRemaining);
                }
            }
        }

        private async Awaitable<float> WaitUntilProjectileFinished(AnimationPvCustomName animName, GridPawnUnit casterUnit,
            LevelCellBase target, PvMnProjectile projectilePrefab)
        {
            if (animName == AnimationPvCustomName.DoNothing)
            {
                await Awaitable.WaitForSecondsAsync(0.1f);
                return 0.1f;
            }

            RaiserAnimationPlayEvent.Raise(casterUnit.transform, animName.ToString());

            var firstExecuteTime = GetPresetAnimationBreakPointFunc.Raise(casterUnit.transform, animName.ToString());

            await Awaitable.WaitForSecondsAsync(firstExecuteTime);

            if (!projectilePrefab)
            {
                return firstExecuteTime;
            }

            var projectile = PvMnProjectilePool.InstantiateProjectile(projectilePrefab);
            await UnitAbilityCoreExtensions.ApplyProjectile(projectile, casterUnit.transform.position, target.transform.position);
            firstExecuteTime += projectile.ProgressDuration;

            return firstExecuteTime;
        }
    }
}