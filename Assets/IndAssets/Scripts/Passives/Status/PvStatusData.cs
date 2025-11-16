using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Status;

namespace IndAssets.Scripts.Passives.Status
{
    public enum PvStatusDisposeType
    {
        LayerOnly,
        DurationOnly,
        Either,
        Both
    }
    
    public class PvStatusData : IBattleStatus
    {
        public string StatusTag { get; internal set; }

        public int Duration { get; internal set; }
        public int Layer { get; internal set; }
        public PvStatusDisposeType DisposeType { get; internal set; } = PvStatusDisposeType.LayerOnly;

        private PvStatusData(Type statusType)
        {
            StatusTag = statusType.Name;
        }
        
        private PvStatusData(string statusTypeName)
        {
            StatusTag = statusTypeName;
        }

        public static PvStatusData CreateStatusData<T>(int duration, int layer,
            PvStatusDisposeType disposeType = PvStatusDisposeType.LayerOnly)
        {
            var output = new PvStatusData(typeof(T))
            {
                Duration = duration,
                Layer = layer,
                DisposeType = disposeType
            };
            return output;
        }

        public static PvStatusData CreateStatusData(string typeName, int duration, int layer)
        {
            var output = new PvStatusData(typeName)
            {
                Duration = duration,
                Layer = layer
            };
            return output;
        }

        public bool IsBeingDisposed()
        {
            return DisposeType switch
            {
                PvStatusDisposeType.LayerOnly => Layer <= 0,
                PvStatusDisposeType.DurationOnly => Duration <= 0,
                PvStatusDisposeType.Either => Layer <= 0 || Duration <= 0,
                PvStatusDisposeType.Both => Layer <= 0 && Duration <= 0,
                _ => true
            };
        }
    }

    public class PvStatusDataCollection : IStatusEffectContainer
    {
        private readonly List<IBattleStatus> _battleStatusList = new();
        private readonly Dictionary<string, PvStatusData> _uniqueStatusTracker = new();
        
        private readonly string _id;
        private readonly string _targetName;

        public PvStatusDataCollection(string unitId, string targetName)
        {
            _id = unitId;
            _targetName = targetName;
        }

        public List<IBattleStatus> GetStatusList() => _battleStatusList;

        public void AddStatus(IBattleStatus statusPrefab)
        {
            AddStatusInstance(statusPrefab);
        }

        private void AddStatusInstance(IBattleStatus statusPrefab)
        {
            var key = statusPrefab.StatusTag;

            if (!_uniqueStatusTracker.TryGetValue(key, out var uniqueStatus))
            {
                uniqueStatus =
                    PvStatusData.CreateStatusData(key, statusPrefab.Duration, statusPrefab.Layer);
                AddUniqueStatus(uniqueStatus);
            }
            else
            {
                uniqueStatus.Duration = statusPrefab.Duration;
                uniqueStatus.Layer += statusPrefab.Layer;
            }
        }

        public void RemoveStatus(IBattleStatus statusPrefab)
        {
            if (!_uniqueStatusTracker.TryGetValue(statusPrefab.StatusTag, out var uniqueStatus))
            {
                return;
            }

            uniqueStatus.Duration -= statusPrefab.Duration;
            uniqueStatus.Layer -= statusPrefab.Layer;
            
            if (uniqueStatus.IsBeingDisposed())
            {
                _uniqueStatusTracker.Remove(statusPrefab.StatusTag);
            }
        }

        public void RemoveStatusDirectly(IBattleStatus statusPrefab)
        {
            if (!_uniqueStatusTracker.TryGetValue(statusPrefab.StatusTag, out var uniqueStatus))
            {
                return;
            }

            _uniqueStatusTracker.Remove(uniqueStatus.StatusTag);
            _battleStatusList.Remove(uniqueStatus);
        }

        public void RemoveStatusByIndex(int index)
        {
            var status = _battleStatusList[index];
            if (!_uniqueStatusTracker.Remove(status.StatusTag))
            {
                return;
            }

            _battleStatusList.RemoveAt(index);
        }

        public void MarkDeductStatus(IBattleStatus statusPrefab)
        {
            if (!_uniqueStatusTracker.TryGetValue(statusPrefab.StatusTag, out var uniqueStatus))
            {
                return;
            }

            uniqueStatus.Duration -= statusPrefab.Duration;
            uniqueStatus.Layer -= statusPrefab.Layer;
        }

        private void AddUniqueStatus(PvStatusData specificStatus)
        {
            _battleStatusList.Add(specificStatus);
            _uniqueStatusTracker.Add(specificStatus.StatusTag, specificStatus);
        }
    }
}