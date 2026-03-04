using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.TacticTool.Formula.Concrete;
using ProjectCI.Utilities.Runtime.Events;
using System.Collections.Generic;

namespace ProjectCI.CoreSystem.Runtime.Commands.Concrete
{
    [StaticInjectableTarget]
    public class PvEnergyObtainCommand : CommandResult
    {
        private readonly static Queue<PvEnergyObtainCommand> _energyCommandsPool = new();
        private int _oldEnergyValue;
        private int _newEnergyValue;

        [Inject] private static readonly IEnergyUpdateEvent _raiserEnergyUpdateEvent;
        
        public override void ApplyCommand(GridPawnUnit fromUnit, GridPawnUnit toUnit)
        {
            _raiserEnergyUpdateEvent.Raise(OwnerId, _oldEnergyValue, _newEnergyValue);
            _energyCommandsPool.Enqueue(this);
        }

        public override void ClearCommand()
        {
            // Empty
        }

        public static void AdjustAndEnqueueEnergy(string resultId, string ownerId, UnitAttributeContainer targetContainer, 
            int deltaValue, AttributeType energyType, Queue<CommandResult> results)
        {
            var currentEnergy = targetContainer.GetAttributeValue(energyType);
            var maxEnergy = FormulaAttributeContainer.MAX_ENERGY_VALUE;
            var calculatedEnergy = currentEnergy + deltaValue;

            if (calculatedEnergy > maxEnergy)
            {
                calculatedEnergy = maxEnergy;
            }
            targetContainer.SetGeneralAttribute(energyType, calculatedEnergy);

            var command = GetCommand();
            command.ResultId = resultId;
            command.OwnerId = ownerId;
            command._oldEnergyValue = currentEnergy;
            command._newEnergyValue = calculatedEnergy;

            results.Enqueue(command);
        }

        private static PvEnergyObtainCommand GetCommand()
        {
            if (!_energyCommandsPool.TryDequeue(out var command))
            {
                command = new PvEnergyObtainCommand();
            }

            return command;
        }
    }
} 