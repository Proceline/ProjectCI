using UnityEngine;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using System;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Concrete
{

    [CreateAssetMenu(fileName = "PvSoDirectedSquareShape", menuName = "ProjectCI Tools/Ability/Shapes/PvSoDirectedSquareShape")]
    public class PvSoDirectedSquareShape : AbilityShape
    {
        [SerializeField] private Vector2Int squareSize;

        public override List<LevelCellBase> GetCellList(GridPawnUnit caster, LevelCellBase cell, int range,
            bool allowBlocked = true, BattleTeam effectedTeam = BattleTeam.None)
        {
            var casterCell = caster.GetCell();
            Vector2Int casterCellIndex = casterCell.GetIndex();
            Vector2Int targetCellIndex = cell.GetIndex();
            
            // Validate that caster and target are aligned (same row or column)
            if (casterCellIndex.x != targetCellIndex.x && casterCellIndex.y != targetCellIndex.y)
            {
                throw new InvalidOperationException("PvSoDirectedSquareShape: Caster and target must be aligned (same row or column)");
            }
            
            List<LevelCellBase> resultCells = new List<LevelCellBase>();
            var grid = TacticBattleManager.GetGrid();
            
            // Determine direction: horizontal (same Y) or vertical (same X)
            bool isHorizontal = casterCellIndex.y == targetCellIndex.y;
            
            // Calculate the square area
            int leftRight = squareSize.x;  // X component controls left-right spread
            int forwardBack = squareSize.y; // Y component controls forward-back spread
            
            // Calculate bounds
            int minX, maxX, minY, maxY;
            
            if (isHorizontal)
            {
                // Horizontal alignment: spread left-right and forward-back along Y axis
                minX = targetCellIndex.x - leftRight;
                maxX = targetCellIndex.x + leftRight;
                
                // Forward direction is away from caster
                int forwardDirection = targetCellIndex.y > casterCellIndex.y ? 1 : -1;
                int forwardEnd = targetCellIndex.y + (forwardBack * forwardDirection);
                int backEnd = targetCellIndex.y - (forwardBack * forwardDirection);
                
                // Ensure we don't go beyond caster position when Y is negative
                if (forwardBack < 0)
                {
                    if (forwardDirection > 0)
                    {
                        backEnd = Mathf.Max(backEnd, casterCellIndex.y);
                    }
                    else
                    {
                        backEnd = Mathf.Min(backEnd, casterCellIndex.y);
                    }
                }
                
                minY = Mathf.Min(forwardEnd, backEnd);
                maxY = Mathf.Max(forwardEnd, backEnd);
            }
            else
            {
                // Vertical alignment: spread left-right along X axis and forward-back along Y axis
                minY = targetCellIndex.y - leftRight;
                maxY = targetCellIndex.y + leftRight;
                
                // Forward direction is away from caster
                int forwardDirection = targetCellIndex.x > casterCellIndex.x ? 1 : -1;
                int forwardEnd = targetCellIndex.x + (forwardBack * forwardDirection);
                int backEnd = targetCellIndex.x - (forwardBack * forwardDirection);
                
                // Ensure we don't go beyond caster position when Y is negative
                if (forwardBack < 0)
                {
                    if (forwardDirection > 0)
                    {
                        backEnd = Mathf.Max(backEnd, casterCellIndex.x);
                    }
                    else
                    {
                        backEnd = Mathf.Min(backEnd, casterCellIndex.x);
                    }
                }
                
                minX = Mathf.Min(forwardEnd, backEnd);
                maxX = Mathf.Max(forwardEnd, backEnd);
            }
            
            // Collect all cells in the calculated area
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Vector2Int cellIndex = new Vector2Int(x, y);
                    try
                    {
                        LevelCellBase targetCell = grid[cellIndex];
                        if (targetCell != null)
                        {
                            resultCells.Add(targetCell);
                        }
                    }
                    catch
                    {
                        // Cell doesn't exist at this index, skip it
                        continue;
                    }
                }
            }
            
            return resultCells;
        }
    }
}
