using System;
using ProjectCI.CoreSystem.Runtime.Abilities;
using UnityEngine;

namespace ProjectCI.Utilities.Runtime.Events
{
    public class AbilitySelectEventParam : IEventParameter
    {
        public PvSoUnitAbility Ability;
    }
    
    // TODO: NOT IN USED FOR NOW
    [CreateAssetMenu(fileName = "AbilitySelect Event", menuName = "ProjectCI Utilities/Events/AbilitySelect Event")]
    public class PvSoAbilitySelectEvent : SoUnityEventBase<AbilitySelectEventParam>, IEventOwner
    {
        private readonly AbilitySelectEventParam _abilitySelectEventParam = new();
        [NonSerialized] private string _identifier = string.Empty;
        
        public void Raise(PvSoUnitAbility ability)
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

        public bool IsGridObject => false;
        public Vector3 Position => Vector3.zero;
        public Vector2Int GridPosition => Vector2Int.zero;
    }
}