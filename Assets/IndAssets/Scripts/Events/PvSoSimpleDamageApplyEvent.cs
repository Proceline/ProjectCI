using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;

namespace ProjectCI.Utilities.Runtime.Events
{
    public class DamageDescriptionParam : IEventParameter
    {
        public int BeforeValue;
        public int AfterValue;
        public int FinalDamageBeforeAdjusted;
        public GridObject Victim;
        public Enum DamageType;
        private readonly HashSet<string> _damageTags = new();

        internal void AssignDetails(int beforeDamage, int afterDamage, int finalDamage, GridObject target, Enum type,
            params string[] tags)
        {
            BeforeValue = beforeDamage;
            AfterValue = afterDamage;
            FinalDamageBeforeAdjusted = finalDamage;
            Victim = target;
            DamageType = type;
            _damageTags.Clear();
            foreach (var tag in tags)
            {
                _damageTags.Add(tag);
            }
        }

        public bool ContainsTag(string tag)
        {
            return _damageTags.Contains(tag);
        }
    }

    [CreateAssetMenu(fileName = "Simple Damage Event", menuName = "ProjectCI Utilities/Events/Simple Damage Event")]
    public class PvSoSimpleDamageApplyEvent : SoUnityEventBase<DamageDescriptionParam>
    {
        private readonly DamageDescriptionParam _damageDescription = new();
        private readonly SimplePawnOwner _damageRecordedOwner = new();

        private class SimplePawnOwner : IEventOwner
        {
            public GridPawnUnit Unit;
            public string EventIdentifier => Unit.ID;
            public bool IsGridObject => true;
            public Vector3 Position => Unit.transform.position;
            public Vector2Int GridPosition => Unit.GetCell().GetIndex();
        }

        public void Raise(int beforeDamage, int afterDamage, int finalDamage, GridPawnUnit damageOwner,
            GridObject targetUnit, Enum damageType, params string[] tags)
        {
            _damageDescription.AssignDetails(beforeDamage, afterDamage, finalDamage, targetUnit, damageType,
                tags);
            _damageRecordedOwner.Unit = damageOwner;
            Raise(_damageRecordedOwner, _damageDescription);
        }
    }
}
