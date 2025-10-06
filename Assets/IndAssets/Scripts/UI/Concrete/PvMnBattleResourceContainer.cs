using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Components;
using UnityEngine;
using UnityEngine.UI;
using System;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.Utilities.Runtime.Events;

namespace ProjectCI.Runtime.GUI.Battle
{
    public class PvMnBattleResourceContainer : BattleHealth
    {
        [NonSerialized] private GameObject _healthBarInstance;
        [NonSerialized] private Slider _healthSlider;
        [NonSerialized] private bool _initialized;
        [SerializeField] private GridObject _followingTarget;

        public void Initialize(GridObject owner, Camera inCamera, GameObject healthBarPrefab)
        {
            // Instantiate health bar UI
            _healthBarInstance = Instantiate(healthBarPrefab);

            // Get UI components
            _healthSlider = _healthBarInstance.GetComponentInChildren<Slider>();
            Canvas canvas = _healthBarInstance.GetComponentInChildren<Canvas>();
            canvas.worldCamera = inCamera;

            RotateHealthBarByCamera(inCamera.transform);
            _followingTarget = owner;
            
            FeLiteGameRules.XRaiserSimpleDamageApplyEvent.RegisterCallback(UpdateHealthViewInfo);
            _initialized = true;
        }

        private void UpdateHealthViewInfo(IEventOwner owner, DamageDescriptionParam damageParams)
        {
            if (_followingTarget != damageParams.Victim) return;
            SetHealth(damageParams.AfterValue);
        }

        private void LateUpdate()
        {
            if (_initialized)
            {
                _healthBarInstance.transform.position = transform.position;
            }
        }

        public void RotateHealthBarByCamera(Transform targetTransform)
        {
            if (!_healthBarInstance) return;
            _healthBarInstance.transform.rotation = targetTransform.rotation;
        }

        public override void SetHealth(int inHealth)
        {
            if (_healthSlider.maxValue < inHealth)
            {
                _healthSlider.maxValue = inHealth;
            }
            _healthSlider.value = inHealth;
        }

        public override void SetMaxHealth(int maxHp)
        {
            _healthSlider.maxValue = maxHp;
        }

        private void OnDestroy()
        {
            if (_healthBarInstance != null)
            {
                Destroy(_healthBarInstance);
            }
            FeLiteGameRules.XRaiserSimpleDamageApplyEvent.UnregisterCallback(UpdateHealthViewInfo);
            _initialized = false;
        }
    }
} 