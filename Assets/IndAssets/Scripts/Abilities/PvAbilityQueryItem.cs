using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Commands.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;
using System.Collections.Generic;

namespace IndAssets.Scripts.Abilities
{
    public class PvAbilityQueryItem<T> where T : GridPawnUnit
    {
        private static readonly Stack<PvAbilityQueryItem<T>> _pool = new Stack<PvAbilityQueryItem<T>>();
        public static PvAbilityQueryItem<T> Get()
        {
            if (_pool.Count > 0)
            {
                return _pool.Pop();
            }
            return new PvAbilityQueryItem<T>();
        }

        public void Release()
        {
            queryOrderForm = default;
            Ability = null;
            holdingOwner = null;
            targetUnit = null;
            enabled = true;
            _calculatedCommands.Clear();

            _pool.Push(this);
        }

        /// <summary>
        /// Can be None, FollowUp or CounterAttack
        /// </summary>
        public PvEnDamageForm queryOrderForm;

        public PvSoUnitAbility Ability { get; private set; }
        public T holdingOwner;
        public T targetUnit;
        public bool enabled = true;

        private readonly Queue<CommandResult> _calculatedCommands = new();
        public Queue<CommandResult> Commands => _calculatedCommands;

        public void SetAbility(PvSoUnitAbility ability, PvEnDamageForm type)
        {
            Ability = ability;
            if (type == PvEnDamageForm.Support)
            {
                queryOrderForm &= ~PvEnDamageForm.Aggressive;
            }
            else if (type == PvEnDamageForm.Aggressive)
            {
                queryOrderForm &= ~PvEnDamageForm.Support;
            }
            
            queryOrderForm |= type;
        }

        public void EnqueueCommand(CommandResult command)
        {
            _calculatedCommands.Enqueue(command);
        }

        public CommandResult DequeueCommand()
        {
            if (_calculatedCommands.Count > 0)
            {
                return _calculatedCommands.Dequeue();
            }
            return null;
        }

        public CommandResult PeekCommand()
        {
            if (_calculatedCommands.Count > 0)
            {
                return _calculatedCommands.Peek();
            }
            return null;
        }

        public static List<PvAbilityQueryItem<TP>> CreateFirstItemList<TP>(TP caster, TP victim)
            where TP : GridPawnUnit
        {
            var outputList = new List<PvAbilityQueryItem<TP>>();

            var queryItem = PvAbilityQueryItem<TP>.Get();
            queryItem.holdingOwner = caster;
            queryItem.targetUnit = victim;

            outputList.Add(queryItem);

            return outputList;
        }

        public static PvAbilityQueryItem<TP> CreateQueryItemIntoList<TP>(List<PvAbilityQueryItem<TP>> existedList, int index = -1)
            where TP : GridPawnUnit
        {
            var queryItem = PvAbilityQueryItem<TP>.Get();
            if (index < 0 || index >= existedList.Count)
            {
                existedList.Add(queryItem);
            }
            else
            {
                existedList.Insert(index, queryItem);
            }

            return queryItem;
        }

        public static void ClearList<TP>(List<PvAbilityQueryItem<TP>> existedList)
            where TP : GridPawnUnit
        {
            foreach (var item in existedList)
            {
                item.Release();
            }
            existedList.Clear();
        }
    }
}