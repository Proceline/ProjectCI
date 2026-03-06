using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Commands.Concrete
{
    /// <summary>
    /// The result of a command execution, can be sent to frontend for animation.
    /// </summary>
    public class PvPushCommand : CommandResult
    {
        private readonly static Queue<PvPushCommand> _commandsPool = new();
        private Vector2Int _stopCellIndex;

        private static CompassDir GetCompassDir(Vector2Int vectorDelta)
        {
            if (vectorDelta.x == 0)
            {
                if (vectorDelta.y > 0)
                {
                    return CompassDir.S;
                }
                else
                {
                    return CompassDir.N;
                }
            }
            else
            {
                if (vectorDelta.x > 0)
                {
                    return CompassDir.E;
                }
                else
                {
                    return CompassDir.W;
                }
            }
        }

        private static PvPushCommand GetCommand()
        {
            if (!_commandsPool.TryDequeue(out var command))
            {
                command = new PvPushCommand();
            }

            return command;
        }

        public override void ApplyCommand(GridPawnUnit fromUnit, GridPawnUnit toUnit)
        {
            var stopCell = TacticBattleManager.GetGrid()[_stopCellIndex];
            toUnit.ForceMoveTo(stopCell);

            _commandsPool.Enqueue(this);
        }

        public override void ClearCommand()
        {
            // Empty
        }

        public static void ExecuteAndAddPushCommand(string resultId, GridPawnUnit fromUnit, GridPawnUnit mainTarget, 
            int distance, LevelCellBase targetCell, Queue<CommandResult> results)
        {
            var victim = targetCell.GetUnitOnCell();
            if (!victim)
            {
                return;
            }

            var mainTargetIndex = mainTarget.GetCell().GetIndex();
            var fromIndex = fromUnit.GetCell().GetIndex();

            var directionDelta = mainTargetIndex - fromIndex;
            if ((directionDelta.x != 0 && directionDelta.y != 0) || (directionDelta.x == 0 && directionDelta.y == 0))
            {
                throw new System.Exception($"ERROR: {directionDelta.ToString()} is not a valid Direction");
            }

            var normalizedDelta = new Vector2(directionDelta.x, directionDelta.y).normalized;
            var unitDelta = new Vector2Int(Mathf.RoundToInt(normalizedDelta.x), Mathf.RoundToInt(normalizedDelta.y));
            var compassDir = GetCompassDir(unitDelta);

            LevelCellBase aimCell = null;
            for (var i = 0; i < distance; i++)
            {
                var nextCell = targetCell.GetAdjacentCell(compassDir);
                if (!nextCell || !nextCell.IsCellAccessible())
                {
                    break;
                }

                aimCell = nextCell;
            }

            if (!aimCell)
            {
                return;
            }

            var command = GetCommand();
            command.ResultId = resultId;
            command.OwnerId = fromUnit.ID;
            command.TargetCellIndex = targetCell.GetIndex();
            command._stopCellIndex = aimCell.GetIndex();

            results.Enqueue(command);
        }
    }
} 