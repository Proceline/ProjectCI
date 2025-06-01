using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using System;
using ProjectCI_Animation.Runtime;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Audio;
using ProjectCI.TacticTool.Formula.Concrete;
namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public class PvMnBattleGeneralUnit : GridPawnUnit
    {
        [NonSerialized] private UnitAnimationManager _animationManager;

        /// <summary>
        /// Called in Package, do not delete
        /// </summary>
        public override void Initalize()
        {
            base.Initalize();
            RuntimeAttributes = new FormulaAttributeContainer();
            SimulatedAttributes = new FormulaAttributeContainer();

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
            RuntimeAttributes.Health.SetValue(unitData.m_Health, unitData.m_Health);
            foreach (var attribute in unitData.originalAttributes)
            {
                RuntimeAttributes.SetGeneralAttribute(attribute.m_AttributeType, attribute.m_Value);
            }
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