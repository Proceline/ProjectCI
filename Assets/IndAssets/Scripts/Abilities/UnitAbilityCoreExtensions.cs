using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Abilities.Projectiles;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Extensions
{
    public static class UnitAbilityCoreExtensions
    {
        public static async Awaitable ApplyResult(PvSoUnitAbility ability, GridPawnUnit casterUnit, LevelCellBase target,
            List<Action<GridPawnUnit, LevelCellBase>> reacts, UnityEvent onNonLogicalComplete = null,
            UnityEvent<GridPawnUnit> onCasterUnitStartAnim = null, UnityEvent<GridPawnUnit> onCasterUnitAfterExecute = null)
        {
            if(ability.GetShape())
            {
                casterUnit.LookAtCell(target);
                // TODO: Need to have a lock
                // TacticBattleManager.AddActionBeingPerformed();

                UnitAbilityAnimation abilityAnimation = ability.abilityAnimation;
                abilityAnimation?.PlayAnimation(casterUnit);
                onCasterUnitStartAnim?.Invoke(casterUnit);

                float firstExecuteTime = abilityAnimation ? abilityAnimation.ExecuteAfterTime(0) : 0.25f;
                await Awaitable.WaitForSecondsAsync(firstExecuteTime);

                if (ability.ProjectilePrefab)
                {
                    var projectile = PvMnProjectilePool.InstantiateProjectile(ability.ProjectilePrefab);
                    projectile.Initialize(casterUnit.transform.position, target.transform.position);
                    while (!projectile.IsProgressEnded)
                    {
                        await Awaitable.NextFrameAsync();
                    }
                    firstExecuteTime += projectile.ProgressDuration;
                }

                onCasterUnitAfterExecute?.Invoke(casterUnit);
                // TODO: Handle Audio
                // AudioPlayData audioData = new AudioPlayData(audioOnExecute);
                // AudioHandler.PlayAudio(audioData, casterUnit.gameObject.transform.position);

                foreach (Action<GridPawnUnit, LevelCellBase> react in reacts)
                {
                    react?.Invoke(casterUnit, target);
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

            onNonLogicalComplete?.Invoke();
        }
    }
}