using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;

namespace IndAssets.Scripts.Events
{
    [StaticInjectableTarget]
    public class PvSoUnitHeldQueryEvent : SoUnityEventBase
    {
        [Inject] internal static readonly PvSoUnitHeldQueryEvent InjectedInstance;
        
        private class PvUnitHeldQueryActionCol
        {
            public CombatingQueryContext QueryContextKey;
            internal Action<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit, Queue<CommandResult>> UnitQueryAction;
        }

        private readonly Dictionary<string, List<PvUnitHeldQueryActionCol>> _unitHeldActionColHash = new();
        
        public void Raise(PvMnBattleGeneralUnit fromUnit, CombatingQueryContext info, PvMnBattleGeneralUnit toUnit, Queue<CommandResult> commands)
        {
            if (!_unitHeldActionColHash.TryGetValue(fromUnit.ID, out var col))
            {
                return;
            }

            var action = col.Find(action => action.QueryContextKey == info);
            action.UnitQueryAction?.Invoke(fromUnit, toUnit, commands);
        }

        public void RegisterCallback(CombatingQueryContext info, PvMnBattleGeneralUnit owner,
            Action<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit, Queue<CommandResult>> callback, bool isMultiple)
        {
            if (!_unitHeldActionColHash.TryGetValue(owner.ID, out var col))
            {
                col = new List<PvUnitHeldQueryActionCol>();
                _unitHeldActionColHash.Add(owner.ID, col);
            }
            
            var index = col.FindIndex(action => action.QueryContextKey == info);
            if (index >= 0)
            {
                if (isMultiple)
                {
                    col[index].UnitQueryAction += callback;
                }
                else
                {
                    col[index].UnitQueryAction = callback;
                }
            }
            else
            {
                col.Add(new PvUnitHeldQueryActionCol
                {
                    QueryContextKey = info,
                    UnitQueryAction = callback
                });
            }
        }

        public void UnregisterCallback(CombatingQueryContext info, PvMnBattleGeneralUnit owner)
        {
            if (!_unitHeldActionColHash.TryGetValue(owner.ID, out var col))
            {
                Debug.LogError($"ERROR: No such QueryApplier registered from <{owner.name}>");
                return;
            }

            var index = col.FindIndex(action => action.QueryContextKey == info);
            if (index >= 0)
            {
                col.RemoveAt(index);
            }
            else
            {
                Debug.LogError($"ERROR: No such QueryApplier/duplicated registered from <{owner.name}>");
            }
        }

        public void UnregisterCallback(CombatingQueryContext info, PvMnBattleGeneralUnit owner,
            Action<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit, Queue<CommandResult>> callback)
        {
            if (!_unitHeldActionColHash.TryGetValue(owner.ID, out var col))
            {
                Debug.LogError($"ERROR: No such QueryApplier registered from <{owner.name}>");
                return;
            }

            var index = col.FindIndex(action => action.QueryContextKey == info);
            if (index >= 0)
            {
                col[index].UnitQueryAction -= callback;
                if (col[index].UnitQueryAction == null)
                {
                    col.RemoveAt(index);
                }
            }
            else
            {
                Debug.LogError($"ERROR: No such QueryApplier/duplicated registered from <{owner.name}>");
            }
        }
    }

    public static class PvUnitHeldQueryExt
    {
        public static void RegisterQueryApply(this PvMnBattleGeneralUnit owner, CombatingQueryContext info,
            Action<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit, Queue<CommandResult>> callback, bool isMultiple = false)
        {
            PvSoUnitHeldQueryEvent.InjectedInstance.RegisterCallback(info, owner, callback, isMultiple);
        }
        
        public static void UnregisterQueryApply(this PvMnBattleGeneralUnit owner, CombatingQueryContext info)
        {
            PvSoUnitHeldQueryEvent.InjectedInstance.UnregisterCallback(info, owner);
        }

        public static void UnregisterQueryApply(this PvMnBattleGeneralUnit owner, CombatingQueryContext info,
            Action<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit, Queue<CommandResult>> callback)
        {
            PvSoUnitHeldQueryEvent.InjectedInstance.UnregisterCallback(info, owner, callback);
        }

        public static void ApplyAdjustedAction(this PvMnBattleGeneralUnit owner, CombatingQueryContext info,
            PvMnBattleGeneralUnit toUnit, Queue<CommandResult> commands)
        {
            PvSoUnitHeldQueryEvent.InjectedInstance.Raise(owner, info, toUnit, commands);
        }
    }
}