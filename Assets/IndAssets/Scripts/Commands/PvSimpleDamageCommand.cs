using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.CoreSystem.Runtime.Commands.Concrete
{
    /// <summary>
    /// The result of a command execution, can be sent to frontend for animation.
    /// </summary>
    public class PvSimpleDamageCommand : PvConcreteCommand
    {
        public int BeforeValue;
        public int AfterValue;
        public int Value;
        public PvEnDamageType DamageType;

        public override void ApplyCommand(GridPawnUnit fromUnit, GridPawnUnit toUnit)
        {
            if (string.IsNullOrEmpty(ExtraInfo))
            {
                FeLiteGameRules.XRaiserSimpleDamageApplyEvent.Raise(BeforeValue, AfterValue, Value, fromUnit,
                    toUnit, DamageType);
            }
            else
            {
                FeLiteGameRules.XRaiserSimpleDamageApplyEvent.Raise(BeforeValue, AfterValue, Value, fromUnit,
                    toUnit, DamageType, ExtraInfo);
            }
        }
    }
} 