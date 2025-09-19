using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using System;
using System.Collections;
using System.Collections.Generic;
using ProjectCI_Animation.Runtime;
using ProjectCI_Animation.Runtime.Concrete;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Animation;
using ProjectCI.TacticTool.Formula.Concrete;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.Runtime.GUI.Battle;
using ProjectCI.Utilities.Runtime.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public partial class PvMnBattleGeneralUnit : GridPawnUnit, IEventOwner
    {
        [NonSerialized] private UnitAnimationManager _animationManager;
        [NonSerialized] private FormulaCollection _formulaCollection;
        private FormulaCollection FormulaCollection => _formulaCollection ??= ServiceLocator.Get<FormulaCollection>();

        private readonly Stack<UnitBattleState> _unitStates = new();
        private readonly ServiceLocator<PvSoAbilityEquipEvent> _raiserServiceForAbilityEquip = new();

        private readonly List<PvSoUnitAbility> _battleAbilities = new();
        
        [NonSerialized]
        private PvSoUnitAbility _currentAbility;

        private Coroutine _rotatingCoroutine;

        private int _maximumMovementPoints;

        public PvSoUnitAbility EquippedAbility
        {
            get
            {
                if (_currentAbility)
                {
                    return _currentAbility;
                }

                var firstWeaponAbility = _battleAbilities.Find(ability =>
                    ability.IsAbilityWeapon() && ability.GetEffectedTeam() == BattleTeam.Hostile);
                EquipAbility(firstWeaponAbility);
                return _currentAbility;
            }
        }
        
        public PvSoUnitAbility DefaultSupport
        {
            get
            {
                return _battleAbilities.Find(ability =>
                    !ability.IsAbilityWeapon() && ability.GetEffectedTeam() == BattleTeam.Friendly);
            }
        }
        
        private void SetFormulaCollection()
        {
            RuntimeAttributes = new FormulaAttributeContainer(FormulaCollection, this);
            SimulatedAttributes = new FormulaAttributeContainer(FormulaCollection, this);
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
            OnMovementPostComplete.AddListener(() => { CurrentMovementPoints = 0; });
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

        void OnDestroy()
        {
            if (_animationManager)
            {
                OnPreStandIdleAnimRequired -= PlayIdleAnimation;
                OnPreMovementAnimRequired -= PlayMovementAnimation;
            }
        }

        private void PlayIdleAnimation()
        {
            _animationManager.PlayLoopAnimation(AnimationIndexName.Idle);
        }

        private void PlayMovementAnimation()
        {
            _animationManager.PlayLoopAnimation(AnimationIndexName.Run);
        }

        private void PlayHitAnimation()
        {
            _animationManager.ForcePlayAnimation(AnimationIndexName.Hit);
        }

        public override void BroadcastActionTriggerByTag(string actionTagName)
        {
            if (!_animationManager) return;
            _animationManager.ForcePlayAnimation(actionTagName);
        }

        public override float GrabActionValueDataByIndexTag(int additionalIndex, params string[] tags)
        {
            if (tags == null) return 0;
            if (!_animationManager || tags.Length < 1) return 0;
            if (tags.Length > 1)
            {
                switch (tags[1])
                {
                    case PvSoPresetAnimationClipExt.AnimationLengthTag:
                        return _animationManager.GetPresetAnimationDuration(tags[0]);
                }
            }
            else
            {
                var breakPoints = _animationManager.GetPresetAnimationBreakPoints(tags[0]);
                if (breakPoints.Length > additionalIndex)
                {
                    return breakPoints[additionalIndex];
                }
            }

            return 0;
        }

        public override void SetUnitData(SoUnitData unitData)
        {
            base.SetUnitData(unitData);

            foreach (var attribute in unitData.originalAttributes)
            {
                RuntimeAttributes.SetGeneralAttribute(attribute.m_AttributeType, attribute.m_Value);
            }

            if (FormulaCollection != null)
            {
                int hitPoint = RuntimeAttributes.GetAttributeValue(FormulaCollection.HealthAttributeType);
                RuntimeAttributes.Health.SetValue(hitPoint, hitPoint);
            }
            else
            {
                RuntimeAttributes.Health.SetValue(10, 10);
            }
        }

        public void EquipAbility(PvSoUnitAbility ability)
        {
            if (!ability.IsAbilityWeapon())
            {
                throw new Exception("ERROR: Ability is not a weapon to EQUIP!");
            }
            _currentAbility = ability;
            _raiserServiceForAbilityEquip.Service.Raise(this, ability);
        }
        
        public void SetupAbilities(ICollection<PvSoUnitAbility> abilities)
        {
            _battleAbilities.Clear();
            foreach (var ability in abilities)
            {
                _battleAbilities.Add(ability);
            }
        }

        public List<PvSoUnitAbility> GetUsableAbilities() => _battleAbilities;

        public List<PvSoUnitAbility> GetAttackAbilities()
        {
            return _battleAbilities.FindAll(ability => ability.GetEffectedTeam() == BattleTeam.Hostile);
        }
        
        public List<PvSoUnitAbility> GetSupportAbilities()
        {
            return _battleAbilities.FindAll(ability => ability.GetEffectedTeam() == BattleTeam.Friendly);
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
            CurrentMovementPoints = 
                RuntimeAttributes.GetAttributeValue(FormulaCollection.MovementAttributeType);
            _maximumMovementPoints = CurrentMovementPoints;
        }

        public void ResetMovementPoints()
        {
            CurrentMovementPoints = _maximumMovementPoints;
        }

        public override List<LevelCellBase> GetAllowedMovementCells()
        {
            // TODO: Change BattleTeam type to enable cross enemy
            return UnitData.m_MovementShape.GetCellList(this, GetCell(), CurrentMovementPoints, UnitData.m_bIsFlying, BattleTeam.Friendly);
        }

        public void ForceMoveToCellImmediately(LevelCellBase targetCell)
        {
            if (targetCell == m_CurrentCell) return;
            SetCurrentCell(targetCell);

            gameObject.transform.position = targetCell.GetAllignPos(this);
            StatusEffectUtils.HandleUnitOnCell(this, targetCell);
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
            Vector3 position = cell.GetAllignPos(this);
            position.y = gameObject.transform.position.y;

            return position;

        }
        #endregion
    }
} 