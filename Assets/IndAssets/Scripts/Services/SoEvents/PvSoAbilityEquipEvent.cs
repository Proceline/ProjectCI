using System;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;

namespace ProjectCI.Utilities.Runtime.Events
{
    [CreateAssetMenu(fileName = "AbilityEquip Event", menuName = "ProjectCI Utilities/Events/AbilityEquip Event")]
    public class PvSoAbilityEquipEvent : SoUnityEventBase<AbilitySelectEventParam>, IService
    {
        private readonly AbilitySelectEventParam _abilitySelectEventParam = new();
        [NonSerialized] private string _identifier = string.Empty;
        
        public void Raise(GridPawnUnit unit, UnitAbilityCore ability)
        {
            if (unit is IEventOwner owner)
            {
                _abilitySelectEventParam.Ability = ability;
                Raise(owner, _abilitySelectEventParam);
            }
        }

        public string EventIdentifier => nameof(PvSoAbilityEquipEvent);

        public void Dispose()
        {
            // Empty
        }

        public void Initialize()
        {
            // Empty
        }

        public void Cleanup()
        {
            // Empty
        }
    }
}