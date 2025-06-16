using System;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;

namespace ProjectCI.Utilities.Runtime.Events
{
    public class AbilitySelectEventParam : IEventParameter
    {
        public UnitAbilityCore Ability;
    }
    
    [CreateAssetMenu(fileName = "AbilitySelect Event", menuName = "ProjectCI Utilities/Events/AbilitySelect Event")]
    public class PvSoAbilitySelectEvent : SoUnityEventBase<AbilitySelectEventParam>, IEventOwner
    {
        private readonly AbilitySelectEventParam _abilitySelectEventParam = new();
        [NonSerialized] private string _identifier = string.Empty;
        
        public void Raise(UnitAbilityCore ability)
        {
            _abilitySelectEventParam.Ability = ability;
            Raise(this, _abilitySelectEventParam);
        }

        public string EventIdentifier
        {
            get
            {
                if (string.IsNullOrEmpty(_identifier))
                {
                    _identifier = Guid.NewGuid().ToString();
                }

                return _identifier;
            }
        }
    }
}