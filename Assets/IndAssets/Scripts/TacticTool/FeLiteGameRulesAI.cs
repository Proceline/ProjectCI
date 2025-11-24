using IndAssets.Scripts.AI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public partial class FeLiteGameRules
    {
        [SerializeField] private PvSoEnemyArrangement aiArrangement;

        public async Awaitable ApplyNextEnemyBehaviour()
        {
#if UNITY_EDITOR
            var allUnitsInBattle = _unitIdToBattleUnitHash.Values;
            var chargeValue = 0;
            foreach (var unit in allUnitsInBattle)
            {
                if (unit.GetTeam() == BattleTeam.Hostile && !unit.IsDead())
                {
                    aiArrangement.AddEnemy(unit, chargeValue++);
                }
            }
#endif
            var nextEnemy = aiArrangement.GetNextEnemy();
            Debug.LogError("Next Enemy exists: " + (nextEnemy == null));
            if (aiArrangement.EnemyThoughtsCollection.TryGetValue(nextEnemy.ID, out var enemyThought))
            {
                ApplyCellUnitToSelectedUnit(nextEnemy.GetCell());
                var result = enemyThought.CalculateBestAction();
                
                if (!result.HasAction)
                {
                    Debug.LogError(result.ShouldTakeRest + " " + (result.MoveToCell == null) + " " + (result.AttackTargetCell == null));
                    return;
                }

                await Awaitable.WaitForSecondsAsync(0.5f);
                
                var targetCell = result.MoveToCell;
                var targetVictim = result.AttackTargetCell;
                
                ApplyMovementToCellForSelectedUnit(targetCell);
                while (nextEnemy.GetCell() != targetCell)
                {
                    await Awaitable.WaitForSecondsAsync(0.25f);
                }
                
                ApplyAbilityToTargetCell(targetVictim);
            }
        }
        
        public async void EditorTestEndRoundToRunAI(BattleTeam endTeam)
        {
#if UNITY_EDITOR
            Debug.LogError(endTeam);
            if (endTeam == BattleTeam.Hostile)
            {
                return;
            }
            await ApplyNextEnemyBehaviour();
#endif
        }
    }
}