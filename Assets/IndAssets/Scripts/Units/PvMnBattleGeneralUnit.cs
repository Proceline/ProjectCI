using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using System;
using System.Collections;
using System.Collections.Generic;
using ProjectCI_Animation.Runtime;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Audio;
using ProjectCI.TacticTool.Formula.Concrete;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.Runtime.GUI.Battle;
using ProjectCI.Utilities.Runtime.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public class PvMnBattleGeneralUnit : GridPawnUnit, IEventOwner
    {
        [NonSerialized] private UnitAnimationManager _animationManager;
        [NonSerialized] private FormulaCollection _formulaCollection;
        private FormulaCollection FormulaCollection => _formulaCollection ??= ServiceLocator.Get<FormulaCollection>();

        private readonly Stack<UnitBattleState> _unitStates = new();
        private readonly ServiceLocator<PvSoAbilityEquipEvent> _abilityEquipEventLocator = new();

        private readonly List<PvSoUnitAbility> _battleAbilities = new();
        
        [NonSerialized]
        private PvSoUnitAbility _currentAbility;

        private Coroutine _rotatingCoroutine;

        public PvSoUnitAbility EquippedAbility
            // { get => _currentAbility.Item1; set => _currentAbility.Item1 = value; }
        {
            get
            {
                if (_currentAbility)
                {
                    return _currentAbility;
                }

                var firstWeaponAbility = _battleAbilities.Find(ability => ability.IsAbilityWeapon());
                EquipAbility(firstWeaponAbility);
                return _currentAbility;
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
        public Vector2 GridPosition => m_CurrentCell.GetIndex();

        /// <summary>
        /// Called in Package, do not delete
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            SetFormulaCollection();

            _animationManager = gameObject.GetComponent<UnitAnimationManager>();
            if (_animationManager)
            {
                OnPreStandIdleAnimRequired += PlayIdleAnimation;
                OnPreMovementAnimRequired += PlayMovementAnimation;
                OnPreHitAnimRequired += PlayHitAnimation;
            }
            
            OnMovementPostComplete.RemoveAllListeners();
            OnMovementPostComplete.AddListener(() =>
            {
                CurrentMovementPoints = 0;
            });
        }

        public void InitializeResourceContainer(Camera uiCamera, GameObject resourceContainerPrefab)
        {
            PvMnBattleResourceContainer container = GetComponent<PvMnBattleResourceContainer>();
            if (container)
            {
                container.OnHealthPreDepleted.AddListener(Kill);
                container.OnHitPreReceived.AddListener(PlayHitVisualResult);
                container.Initialize(uiCamera, resourceContainerPrefab);
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
                OnPreHitAnimRequired -= PlayHitAnimation;
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
            _abilityEquipEventLocator.Service.Raise(this, ability);
        }
        
        public void SetupAbilities(ICollection<PvSoUnitAbility> abilities)
        {
            _battleAbilities.Clear();
            foreach (var ability in abilities)
            {
                _battleAbilities.Add(ability);
            }
        }

        public List<PvSoUnitAbility> GetUsableAbilities()
        {
            return _battleAbilities;
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
        }

        public override List<LevelCellBase> GetAllowedMovementCells()
        {
            // TODO: Change BattleTeam type to enable cross enemy
            return UnitData.m_MovementShape.GetCellList(this, GetCell(), CurrentMovementPoints, UnitData.m_bIsFlying, BattleTeam.Friendly);
        }

        private void Kill()
        {
            // TODO: Consider Clear States
            // ClearStates();

            if ( m_CurrentCell )
            {
                m_CurrentCell.SetObjectOnCell(null);
                m_CurrentCell = null;
            }

            // TODO: Handle isDead, m_bIsDead = true;

            SetVisible(false);
            CheckCellVisibility();

            // TODO: HandleDeath();

            AudioClip clip = GetUnitData().m_DeathSound;
            if (clip)
            {
                AudioPlayData audioData = new AudioPlayData(clip);
                AudioHandler.PlayAudio(audioData, gameObject.transform.position);
            }
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