using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using System;
using System.Collections;
using System.Collections.Generic;
using IndAssets.Scripts.Passives.Status;
using ProjectCI_Animation.Runtime;
using ProjectCI_Animation.Runtime.Concrete;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.TacticTool.Formula.Concrete;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Status;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.Runtime.GUI.Battle;
using ProjectCI.Utilities.Runtime.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public enum UnitTypeValue : int
    {
        Melee = 0,
        Ranged = 1,
        Magic = 2
    }

    public partial class PvMnBattleGeneralUnit : GridPawnUnit, IEventOwner
    {
        [NonSerialized] private UnitAnimationManager _animationManager;
        private static FormulaCollection _formulaCollection;
        private static FormulaCollection FormulaCollection => _formulaCollection ??= ServiceLocator.Get<FormulaCollection>();

        private readonly Stack<UnitBattleState> _unitStates = new();
        private readonly List<PvSoUnitAbility> _battleAbilities = new();

        [NonSerialized]
        private PvSoUnitAbility _attackAbility;

        [NonSerialized]
        private PvSoUnitAbility _followUpAbility;

        [NonSerialized]
        private PvSoUnitAbility _counterAbility;

        [NonSerialized]
        private PvSoUnitAbility _supportAbility;

        public PvSoUnitAbility AttackAbility => _attackAbility;
        public PvSoUnitAbility FollowUpAbility => _followUpAbility;
        public PvSoUnitAbility CounterAbility => _counterAbility;
        public PvSoUnitAbility SupportAbility => _supportAbility;

        private Coroutine _rotatingCoroutine;

        private int _currentMovementPoints;
        private int _currentActionPoints = 1;

        private PvStatusDataCollection _statusCollection;

        private void SetFormulaCollection()
        {
            RuntimeAttributes = new FormulaAttributeContainer(FormulaCollection, this);
        }

        public override void GenerateNewID()
        {
            ID = Guid.NewGuid().ToString();
        }

        public string EventIdentifier => ID;
        public bool IsGridObject => true;
        public Vector3 Position => transform.position;
        public Vector2Int GridPosition => m_CurrentCell.GetIndex();

        /// <summary>
        /// Called in Package, do not delete
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            SetFormulaCollection();

            var animator = GetComponent<PvMnFunctionalAnimator>();
            if (!animator)
            {
                animator = GetComponentInChildren<PvMnFunctionalAnimator>();
            }

            if (animator)
            {
                animator.Initialize(this);
            }

            _animationManager = animator;

            if (_animationManager)
            {
                OnPreStandIdleAnimRequired += PlayIdleAnimation;
                OnPreMovementAnimRequired += PlayMovementAnimation;
            }

            OnMovementPostComplete.RemoveAllListeners();
        }

        public void InitializeResourceContainer(Camera uiCamera, GameObject resourceContainerPrefab)
        {
            PvMnBattleResourceContainer container = GetComponent<PvMnBattleResourceContainer>();
            if (container)
            {
                container.Initialize(this, uiCamera, resourceContainerPrefab);
                container.SetHealth(RuntimeAttributes.Health.CurrentValue);
                container.SetMaxHealth(RuntimeAttributes.Health.MaxValue);
            }
        }

        private void OnDestroy()
        {
            if (_animationManager)
            {
                OnPreStandIdleAnimRequired -= PlayIdleAnimation;
                OnPreMovementAnimRequired -= PlayMovementAnimation;
            }
        }

        public override bool IsDead()
        {
            return RuntimeAttributes.Health.CurrentValue <= 0;
        }

        public override int GetCurrentMovementPoints() => _currentMovementPoints;

        private void PlayIdleAnimation()
        {
            _animationManager.PlayLoopAnimation(AnimationIndexName.Idle);
        }

        private void PlayMovementAnimation()
        {
            _animationManager.PlayLoopAnimation(AnimationIndexName.Run);
        }

        public void SetupAttackAbility(PvSoUnitAbility ability)
        {
            _attackAbility = ability;
        }

        public void SetupFollowUpAbility(PvSoUnitAbility ability)
        {
            _followUpAbility = ability;
        }

        public void SetupCounterAbility(PvSoUnitAbility ability)
        {
            _counterAbility = ability;
        }

        public void SetupSupportAbility(PvSoUnitAbility ability)
        {
            _supportAbility = ability;
        }

        /// <summary>
        /// Do not use in undeprecated Scripts
        /// </summary>
        /// <returns></returns>
        public List<PvSoUnitAbility> GetUsableAbilities() => new List<PvSoUnitAbility>()
        {
            _attackAbility, _followUpAbility, _counterAbility, _supportAbility
        };

        public override IStatusEffectContainer GetStatusEffectContainer()
        {
            return _statusCollection ??= new PvStatusDataCollection(ID, name);
        }

        public override UnitBattleState GetCurrentState()
        {
            if (_unitStates.TryPeek(out var result))
            {
                return result;
            }

            return UnitBattleState.Idle;
        }

        public override void AddState(UnitBattleState state)
        {
            _unitStates.Push(state);
        }

        public override void RemoveLastState()
        {
            if (!_unitStates.TryPop(out _))
            {
                throw new IndexOutOfRangeException("ERROR: No state left!");
            }
        }

        public override void ClearStates()
        {
            _unitStates.Clear();
        }

        public override void HandleTurnStarted()
        {
            _currentMovementPoints = 
                RuntimeAttributes.GetAttributeValue(FormulaCollection.MovementAttributeType);
            SetCurrentActionPoints(1);
        }

        public override void SetCurrentMovementPoints(int movePoint)
        {
            _currentMovementPoints = movePoint;
        }

        public override List<LevelCellBase> GetAllowedMovementCells()
        {
            // TODO: Change BattleTeam type to enable cross enemy
            return UnitData.m_MovementShape.GetCellList(this, GetCell(), _currentMovementPoints, UnitData.m_bIsFlying, BattleTeam.Friendly);
        }

        public void ForceMoveToCellImmediately(LevelCellBase targetCell)
        {
            if (targetCell == m_CurrentCell) return;
            SetCurrentCell(targetCell);

            gameObject.transform.position = targetCell.GetAlignPos(this);
        }

        public override int GetCurrentActionPoints()
        {
            return _currentActionPoints;
        }

        public override void SetCurrentActionPoints(int actionPoint)
        {
            _currentActionPoints = actionPoint;
        }

        #region Rotator
        public override void LookAtCell(LevelCellBase targetCell)
        {
            if (targetCell && ShouldLookAtTargets())
            {
                if (_rotatingCoroutine != null)
                {
                    StopCoroutine(_rotatingCoroutine);
                    _rotatingCoroutine = null;
                }
                _rotatingCoroutine = StartCoroutine(RotateTowards(GetCellLookAtPos(targetCell), 1000));
            }
        }

        private IEnumerator RotateTowards(Vector3 targetPos, float speed)
        {
            Transform currentTrans = gameObject.transform;
            Vector3 dir = targetPos - transform.position;
            if (dir == Vector3.zero)
            {
                _rotatingCoroutine = null;
                yield break;
            }
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);

            while (Quaternion.Angle(currentTrans.rotation, targetRot) > 0.1f)
            {
                currentTrans.rotation = Quaternion.RotateTowards(
                    currentTrans.rotation,
                    targetRot,
                    speed * Time.deltaTime
                );

                yield return Awaitable.NextFrameAsync();
            }
            
            currentTrans.rotation = targetRot;
            _rotatingCoroutine = null;
        }

        private Vector3 GetCellLookAtPos(LevelCellBase cell)
        {
            if (!cell) return Vector3.zero;
            Vector3 position = cell.GetAlignPos(this);
            position.y = gameObject.transform.position.y;

            return position;

        }
        #endregion
    }
} 