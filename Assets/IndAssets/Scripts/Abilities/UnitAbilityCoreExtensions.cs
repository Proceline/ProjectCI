using System;
using System.Collections.Generic;
using IndAssets.Scripts.Random;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Abilities.Projectiles;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using ProjectCI.TacticTool.Formula.Concrete;
using ProjectCI.Utilities.Runtime.Functions;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Extensions
{
    [StaticInjectableTarget]
    public static class UnitAbilityCoreExtensions
    {
        public const string CriticalExtraInfoHint = "Critical";
        public const string MissExtraInfoHint = "Miss";
        public const string HealExtraInfoHint = "Heal";

        public const string AlwaysHitCondition = "AlwaysHit";
        
        [Inject] private static PvSoOutBooleanFunction _raiserIsAnimatingProgressFunc;
        private static readonly ServiceLocator<PvSoRandomSeedCentre> RandomSeedProvider = new();
        private static readonly ServiceLocator<FormulaCollection> FormulaCollection = new();
        
        private const float AnimatingPendingInterval = 0.125f;

        public static async Awaitable WaitUntilLockReleased(GridPawnUnit casterUnit)
        {
            while (_raiserIsAnimatingProgressFunc.Get(casterUnit))
            {
                await Awaitable.WaitForSecondsAsync(AnimatingPendingInterval);
            }
        }

        public static async Awaitable<float> WaitUntilProjectileFinished(this PvSoUnitAbility ability, GridPawnUnit casterUnit,
            LevelCellBase target)
        {
            var abilityAnimation = ability.abilityAnimation;
            abilityAnimation?.PlayAnimation(casterUnit);

            var firstExecuteTime = abilityAnimation ? abilityAnimation.ExecuteAfterTime(0) : 0.25f;
            await Awaitable.WaitForSecondsAsync(firstExecuteTime);

            if (!ability.ProjectilePrefab) return firstExecuteTime;
            var projectile = PvMnProjectilePool.InstantiateProjectile(ability.ProjectilePrefab);
            await ApplyProjectile(projectile, casterUnit.transform.position, target.transform.position);
            firstExecuteTime += projectile.ProgressDuration;

            return firstExecuteTime;
        }

        public static void HandleAbilityParam(this PvSoUnitAbility ability, GridPawnUnit caster,
            GridPawnUnit mainTarget,
            Queue<CommandResult> results)
        {
            if (caster.IsDead())
            {
                return;
            }

            var resultId = Guid.NewGuid().ToString();

            var fromContainer = caster.RuntimeAttributes;
            var caughtSeedValue = RandomSeedProvider.Service.GetNextRandomNumber(0, 100);
            var dcFinal = fromContainer.GetAttributeValue(ability.DcAttribute) + caughtSeedValue;

            List<LevelCellBase> effectedCells = ability.GetEffectedCells(caster, mainTarget.GetCell());
            foreach (AbilityParamBase param in ability.GetParameters())
            {
                foreach (var cell in effectedCells)
                {
                    var cellUnit = cell.GetUnitOnCell();
                    int delta = 0;
                    if (cellUnit)
                    {
                        var acFinal = cellUnit.RuntimeAttributes.GetAttributeValue(ability.AcAttribute) + 10;
                        delta = dcFinal - acFinal;
                    }

                    if (delta > 0)
                    {
                        var criticalAttribute =
                            100 - fromContainer.GetAttributeValue(FormulaCollection.Service.CriticalAttributeType);
                        if (caughtSeedValue >= criticalAttribute)
                        {
                            delta = 100;
                        }
                    }

                    param.Execute(resultId, ability, caster, mainTarget, cell, results, delta);
                }
            }
        }

        private static async Awaitable ApplyProjectile(PvMnProjectile projectile, Vector3 departure, Vector3 dest)
        {
            projectile.Initialize(departure, dest);
            while (!projectile.IsProgressEnded)
            {
                await Awaitable.NextFrameAsync();
            }
        }
    }
}