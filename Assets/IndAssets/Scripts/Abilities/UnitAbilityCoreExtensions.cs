using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Abilities.Projectiles;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
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
        
        [Inject] private static PvSoOutBooleanFunction _raiserIsAnimatingProgressFunc;
        private const float AnimatingPendingInterval = 0.125f;
        
        public static async Awaitable ApplyAnimationProcess(PvSoUnitAbility ability, GridPawnUnit casterUnit,
            LevelCellBase target, Queue<Action<GridPawnUnit>> reacts)
        {
            if (ability.GetShape())
            {
                casterUnit.LookAtCell(target);
                
                while (_raiserIsAnimatingProgressFunc.Get(casterUnit))
                {
                    await Awaitable.WaitForSecondsAsync(AnimatingPendingInterval);
                }

                UnitAbilityAnimation abilityAnimation = ability.abilityAnimation;
                abilityAnimation?.PlayAnimation(casterUnit);

                float firstExecuteTime = abilityAnimation ? abilityAnimation.ExecuteAfterTime(0) : 0.25f;
                await Awaitable.WaitForSecondsAsync(firstExecuteTime);

                if (ability.ProjectilePrefab)
                {
                    var projectile = PvMnProjectilePool.InstantiateProjectile(ability.ProjectilePrefab);
                    await ApplyProjectile(projectile, casterUnit.transform.position, target.transform.position);
                    firstExecuteTime += projectile.ProgressDuration;
                }

                // TODO: Handle Audio
                // AudioPlayData audioData = new AudioPlayData(audioOnExecute);
                // AudioHandler.PlayAudio(audioData, casterUnit.gameObject.transform.position);

                while (reacts.TryDequeue(out var reactAction))
                {
                    reactAction?.Invoke(casterUnit);
                }

                if (abilityAnimation)
                {
                    float timeRemaining = abilityAnimation.GetAnimationLength() - firstExecuteTime;
                    timeRemaining = Mathf.Max(0, timeRemaining);

                    await Awaitable.WaitForSecondsAsync(timeRemaining);
                }

                // TODO: Need a end of lock
                // TacticBattleManager.RemoveActionBeingPerformed();
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