using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using System;
using System.Collections.Generic;
using ProjectCI_Animation.Runtime;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Audio;
using ProjectCI.TacticTool.Formula.Concrete;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.Utilities.Runtime.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public class PvMnBattleGeneralUnit : GridPawnUnit, IEventOwner
    {
        [NonSerialized] private UnitAnimationManager _animationManager;
        [NonSerialized] private FormulaCollection _formulaCollection;
        private FormulaCollection FormulaCollection => _formulaCollection ??= ServiceLocator.Get<FormulaCollection>();

        private readonly Stack<UnitBattleState> _unitStates = new();

        private readonly ServiceLocator<PvSoUnitBattleStateEvent> _stateEventLocator = new();
        private readonly ServiceLocator<PvSoUnitSelectEvent> _selectEventLocator = new();
        private readonly ServiceLocator<PvSoAbilityEquipEvent> _abilityEquipEventLocator = new();
        
        
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
            _stateEventLocator.Service.RegisterCallback(AdjustState);
            _abilityEquipEventLocator.Service.RegisterCallback(EquipAbilityWithParam);

            _animationManager = gameObject.GetComponent<UnitAnimationManager>();
            if (_animationManager)
            {
                OnPreStandIdleAnimRequired += PlayIdleAnimation;
                OnPreMovementAnimRequired += PlayMovementAnimation;
                OnPreHitAnimRequired += PlayHitAnimation;
            }
            
            _selectEventLocator.Service.RegisterCallback(RespondOnManagerSelectUnit);
            OnMovementPostComplete.RemoveAllListeners();
            OnMovementPostComplete.AddListener(() =>
            {
                m_CurrentMovementPoints = 0;
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

        protected override void DestroyObj()
        {
            _stateEventLocator.Service.UnregisterCallback(AdjustState);
            _selectEventLocator.Service.UnregisterCallback(RespondOnManagerSelectUnit);
            _abilityEquipEventLocator.Service.UnregisterCallback(EquipAbilityWithParam);
            
            if (_animationManager)
            {
                OnPreStandIdleAnimRequired -= PlayIdleAnimation;
                OnPreMovementAnimRequired -= PlayMovementAnimation;
                OnPreHitAnimRequired -= PlayHitAnimation;
            }
            base.DestroyObj();
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

        public override void SetupAbility(UnitAbilityCore ability)
        {
            ResetCells();
            
            if (ability)
            {
                List<LevelCellBase> editedAbilityCells = ability.Setup(this);
                EditedCells.AddRange(editedAbilityCells);
            }

            if (ability.IsAbilityAbleToEquip())
            {
                _abilityEquipEventLocator.Service.Raise(this, ability);
            }
            else
            {
                // TODO: Equip related weapon
            }
        }
        
        private void EquipAbilityWithParam(IEventOwner owner, AbilitySelectEventParam selectParam)
        {
            if (owner.EventIdentifier == EventIdentifier)
            {
                CurrentAbility = selectParam.Ability;
            }
        }

        public override UnitBattleState GetCurrentState()
        {
            if (_unitStates.TryPeek(out var result))
            {
                return result;
            }

            return UnitBattleState.Idle;
        }

        private void AdjustState(IEventOwner unit, UnitStateEventParam transport)
        {
            if (unit.EventIdentifier != EventIdentifier)
                return;
            
            switch (transport.behaviour)
            {
                case UnitStateBehaviour.Clear:
                    _unitStates.Clear();
                    break;
                case UnitStateBehaviour.Adding:
                    _unitStates.Push(transport.battleState);
                    break;
                case UnitStateBehaviour.Popping:
                    if (_unitStates.TryPop(out var result))
                    {
                        var currentState = GetCurrentState();
                        _stateEventLocator.Service.Raise(this, currentState, UnitStateBehaviour.Emphasis);
                    }
                    break;
            }
        }

        public override void AddState(UnitBattleState state)
        {
            _stateEventLocator.Service.Raise(this, state, UnitStateBehaviour.Adding);
        }

        public override void RemoveLastState()
        {
            var currentState = GetCurrentState();
            _stateEventLocator.Service.Raise(this, currentState, UnitStateBehaviour.Popping);
        }

        public override void ClearStates()
        {
            _stateEventLocator.Service.Raise(this, UnitBattleState.Idle, UnitStateBehaviour.Clear);
            ResetCells();
        }

        public override void HandleTurnStarted()
        {
            m_CurrentAbilityPoints = 1;
            m_CurrentMovementPoints = 
                RuntimeAttributes.GetAttributeValue(FormulaCollection.MovementAttributeType);
        }

        public override List<LevelCellBase> GetAllowedMovementCells()
        {
            // TODO: Change BattleTeam type to enable cross enemy
            return UnitData.m_MovementShape.GetCellList(this, GetCell(), m_CurrentMovementPoints, UnitData.m_bIsFlying, BattleTeam.Friendly);
        }

        private void Kill()
        {
            ClearStates();

            if ( m_CurrentCell )
            {
                m_CurrentCell.SetObjectOnCell(null);
                m_CurrentCell = null;
            }

            m_bIsDead = true;

            SetVisible( false );

            CheckCellVisibility();

            HandleDeath();

            AudioClip clip = GetUnitData().m_DeathSound;
            if (clip)
            {
                AudioPlayData audioData = new AudioPlayData(clip);
                AudioHandler.PlayAudio(audioData, gameObject.transform.position);
            }

            if (TacticBattleManager.IsActionBeingPerformed())
            {
                TacticBattleManager.BindToOnFinishedPerformedActions(DestroyObj);
            }
            else
            {
                DestroyObj();
            }
        }

        private void RespondOnManagerSelectUnit(IEventOwner owner, UnitSelectEventParam selectInfo)
        {
            if (selectInfo.GridPawnUnit == null || selectInfo.Behaviour == UnitSelectBehaviour.Deselect)
            {
                ClearStates();
            }

            if (selectInfo.GridPawnUnit == this && selectInfo.Behaviour == UnitSelectBehaviour.Select)
            {
                SetupMovement();
            }
        }
    }
} 