using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Components;
using ProjectCI.CoreSystem.Runtime.Attributes;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public class PvMnBattleResourceContainer : BattleHealth
    {
        [NonSerialized] private GameObject _healthBarInstance;
        [NonSerialized] private Slider _healthSlider;

        public void Initialize(Camera InCamera, GameObject healthBarPrefab)
        {
            if (healthBarPrefab != null)
            {
                // Instantiate health bar UI
                _healthBarInstance = Instantiate(healthBarPrefab);
                
                // Get UI components
                _healthSlider = _healthBarInstance.GetComponentInChildren<Slider>();
                Canvas canvas = _healthBarInstance.GetComponentInChildren<Canvas>();
                canvas.worldCamera = InCamera;
                
                RotateHealthBarByCamera(InCamera);
            }
        }

        void LateUpdate()
        {
            if (_healthBarInstance != null)
            {
                _healthBarInstance.transform.position = transform.position;
            }
        }

        private void RotateHealthBarByCamera(Camera InCamera)
        {
            if (_healthBarInstance != null)
            {
                _healthBarInstance.transform.rotation = InCamera.transform.rotation;
            }
        }

        public override void SetHealth(int InHealth)
        {
            if (_healthSlider.maxValue < InHealth)
            {
                _healthSlider.maxValue = InHealth;
            }
            _healthSlider.value = InHealth;
        }

        public override void SetMaxHealth(int InMaxHealth)
        {
            _healthSlider.maxValue = InMaxHealth;
        }

        public override void ReceiveHealthDamage(int InDamage)
        {

        }

        private void OnDestroy()
        {
            if (_healthBarInstance != null)
            {
                Destroy(_healthBarInstance);
            }
        }
    }
} 