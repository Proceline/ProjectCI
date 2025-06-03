using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using System;
using ProjectCI_Animation.Runtime;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Audio;
using ProjectCI.TacticTool.Formula.Concrete;
using ProjectCI.CoreSystem.Runtime.Services;
namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public class PvMnBattleGeneralUnit : GridPawnUnit
    {
        [NonSerialized] private UnitAnimationManager _animationManager;
        [NonSerialized] private FormulaCollection _formulaCollection;
        private FormulaCollection FormulaCollection => _formulaCollection ?? (_formulaCollection = ServiceLocator.Get<FormulaCollection>());

        private void SetFormulaCollection()
        {
            RuntimeAttributes = new FormulaAttributeContainer(FormulaCollection);
            SimulatedAttributes = new FormulaAttributeContainer(FormulaCollection);
        }

        /// <summary>
        /// Called in Package, do not delete
        /// </summary>
        public override void Initalize()
        {
            base.Initalize();
            SetFormulaCollection();

            _animationManager = gameObject.GetComponent<UnitAnimationManager>();
            if (_animationManager)
            {
                OnPreStandIdleAnimRequired += PlayIdleAnimation;
                OnPreMovementAnimRequired += PlayMovementAnimation;
                OnPreHitAnimRequired += PlayHitAnimation;
            }
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

        public override void HandleTurnStarted()
        {
            m_CurrentAbilityPoints = 1;
            m_CurrentMovementPoints = 
                RuntimeAttributes.GetAttributeValue(FormulaCollection.MovementAttributeType);
        }

        protected override void HandleTraversePreFinished()
        {
            
        }

        private void Kill()
        {
            CleanUp();

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
    }
} 