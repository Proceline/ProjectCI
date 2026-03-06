using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;
using System.Collections.Generic;

namespace ProjectCI.CoreSystem.Runtime.Commands.Concrete
{
    /// <summary>
    /// The result of a command execution, can be sent to frontend for animation.
    /// </summary>
    [StaticInjectableTarget]
    public class PvSimpleDamageCommand : CommandResult
    {
        private readonly static Queue<PvSimpleDamageCommand> _commandsPool = new();

        public int BeforeValue;
        public int AfterValue;
        public int Value;
        public PvEnDamageType DamageType;
        public PvEnDamageForm DamageForm;
        public PvEnDamageReact DamageReact;

        [Inject] private static readonly IPvDamageLikeApplyEvent raiserDamageApplyEvent;

        public override void ApplyCommand(GridPawnUnit fromUnit, GridPawnUnit toUnit)
        {
            raiserDamageApplyEvent.Raise(fromUnit.ID, toUnit.ID, BeforeValue, AfterValue, Value, DamageType, DamageForm, DamageReact);
            _commandsPool.Enqueue(this);
        }

        public override void ClearCommand()
        {
            // Empty
        }

        private static PvSimpleDamageCommand GetCommand()
        {
            if (!_commandsPool.TryDequeue(out var command))
            {
                command = new PvSimpleDamageCommand();
            }

            return command;
        }

        public static void AddDamageLikeCommandToHealth(string resultId, GridPawnUnit fromUnit, LevelCellBase targetCell, 
            UnitAttributeContainer toContainer, int damage, 
            PvEnDamageForm damageForm, PvEnDamageType damageType, PvEnDamageReact reaction, Queue<CommandResult> results)
        {
            var beforeHealth = toContainer.Health.CurrentValue;
            if (!damageForm.HasFlag(PvEnDamageForm.Support))
            {
                toContainer.Health.ModifyValue(-damage);
            }
            else
            {
                toContainer.Health.ModifyValue(damage);
            }

            var afterHealth = toContainer.Health.CurrentValue;

            var command = GetCommand();
            command.ResultId = resultId;
            command.OwnerId = fromUnit.ID;
            command.TargetCellIndex = targetCell.GetIndex();
            command.BeforeValue = beforeHealth;
            command.AfterValue = afterHealth;
            command.Value = damage;
            command.DamageType = damageType;
            command.DamageForm = damageForm;
            command.DamageReact = reaction;

            results.Enqueue(command);
        }
    }
} 