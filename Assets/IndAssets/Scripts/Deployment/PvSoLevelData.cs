using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Deployment
{
    [Flags]
    public enum PvLevelTargetType
    {
        None = 0,
        KillAll = 1 << 0,
        KillDetermined = 1 << 1,
        LiveDetermined = 1 << 2,
        DefendRounds = 1 << 3,
        OccupiedAll = 1 << 4,
        Escape = 1 << 5
    }

    public enum PvTargetCompleteCondition
    {
        Uncompleted,
        Failed,
        Completed
    }

    /// <summary>
    /// Level data containing deployment slot information for friendly and hostile units
    /// </summary>
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "ProjectCI/Deployment/Level Data", order = 1)]
    public class PvSoLevelData : ScriptableObject
    {
        public static PvSoLevelData LoadingLevel { get; private set; }

        [SerializeField] private PvLevelTargetType targetType;
        [SerializeField] private List<SoUnitData> eraseTargets = new();
        [NonSerialized] private HashSet<SoUnitData> _eraseTargetsHash;

        [SerializeField] private Vector3 scanStartPivot;
        public int gridWidth;
        public int gridHeight;

        [SerializeField] private int minimumPlayersCount;
        [SerializeField] private List<PvSoBattleUnitData> requiredUnits;
        [SerializeField] private int maximumPlayersCount;

        [SerializeField] private Vector3 cameraPosition;

        [Header("Friendly Deployment Slots")]
        [SerializeField] private List<Vector3> friendlySlots = new List<Vector3>();

        public Vector3 LevelStartPosition => scanStartPivot;

        /// <summary>
        /// Get all friendly deployment slots
        /// </summary>
        public List<Vector3> FriendlySlots => friendlySlots;

        /// <summary>
        /// Focus Camera position, assign LoadingLevel
        /// </summary>
        public void PutCameraOnPosition()
        {
            var mainCamera = Camera.main;
            var cameraParent = mainCamera.transform.parent;
            var targetRoot = mainCamera.transform;
            while (cameraParent != null)
            {
                targetRoot = cameraParent;
                cameraParent = cameraParent.parent;
            }

            targetRoot.position = cameraPosition;

            LoadingLevel = this;
        }

        public bool CheckIfPawnsMeetRequirement(Func<PvSoBattleUnitData, bool> checker, int totalCount)
        {
            foreach (var unit in requiredUnits)
            {
                if (!checker.Invoke(unit))
                {
                    return false;
                }
            }

            if (totalCount < minimumPlayersCount || totalCount > maximumPlayersCount)
            {
                return false;
            } 
            
            return true;
        }

        public PvTargetCompleteCondition CheckIfLevelCompleted(IDictionary<string, PvMnBattleGeneralUnit> units)
        {
            if (targetType == PvLevelTargetType.None)
            {
                return PvTargetCompleteCondition.Failed;
            }

            if (targetType.HasFlag(PvLevelTargetType.LiveDetermined))
            {

            }
            if (targetType.HasFlag(PvLevelTargetType.KillAll))
            {
                foreach(var unitPair in units)
                {
                    var unit = unitPair.Value;
                    if (unit.GetTeam() == BattleTeam.Hostile)
                    {
                        if (!unit.IsDead())
                        {
                            return PvTargetCompleteCondition.Uncompleted;
                        }
                    }
                }

                return PvTargetCompleteCondition.Completed;
            }
            else if (targetType.HasFlag(PvLevelTargetType.KillDetermined))
            {
                if (_eraseTargetsHash == null || _eraseTargetsHash.Count != eraseTargets.Count)
                {
                    _eraseTargetsHash = new HashSet<SoUnitData>(eraseTargets);
                }

                foreach(var unitPair in units)
                {
                    var unit = unitPair.Value;
                    if (_eraseTargetsHash.Contains(unit.GetUnitData()) 
                        && unit.GetTeam() == BattleTeam.Hostile 
                        && !unit.IsDead())
                    {
                        return PvTargetCompleteCondition.Uncompleted;
                    }
                }
            }
            
            return PvTargetCompleteCondition.Completed;
        }
    }
}
