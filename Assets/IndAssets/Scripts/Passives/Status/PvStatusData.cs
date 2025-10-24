using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Status;

namespace IndAssets.Scripts.Passives.Status
{
    public class PvStatusData : IBattleStatus
    {
        public string StatusTag { get; internal set; }

        public int Duration { get; internal set; }
        public int Layer { get; internal set; }

        private PvStatusData(Type statusType)
        {
            StatusTag = statusType.Name;
        }
        
        private PvStatusData(string statusTypeName)
        {
            StatusTag = statusTypeName;
        }
        
        public static PvStatusData CreateStatusData<T>(int duration, int layer)
        {
            var output = new PvStatusData(typeof(T))
            {
                Duration = duration,
                Layer = layer
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
    }

    public class PvStatusDataCollection : IStatusEffectContainer
    {
        private readonly List<IBattleStatus> _battleStatusList = new();
        private readonly Dictionary<string, PvStatusData> _uniqueStatusTracker = new();

        public List<IBattleStatus> GetStatusList() => _battleStatusList;

        public void AddStatus(IBattleStatus status)
        {
            AddStatusInstance(status);
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

        public void RemoveStatus(IBattleStatus status)
        {
            if (!_uniqueStatusTracker.TryGetValue(status.StatusTag, out var uniqueStatus))
            {
                _battleStatusList.Remove(status);
            }
            else
            {
                uniqueStatus.Duration -= status.Duration;
                uniqueStatus.Layer -= status.Layer;
                if (uniqueStatus.Layer <= 0 && uniqueStatus.Duration <= 0)
                {
                    _battleStatusList.Remove(status);
                }
            }
        }

        private void AddUniqueStatus(PvStatusData specificStatus)
        {
            _battleStatusList.Add(specificStatus);
            _uniqueStatusTracker.Add(specificStatus.StatusTag, specificStatus);
        }
    }
}