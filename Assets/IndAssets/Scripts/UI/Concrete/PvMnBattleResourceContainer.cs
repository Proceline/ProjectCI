using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Components;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace ProjectCI.Runtime.GUI.Battle
{
    public class PvMnBattleResourceContainer : BattleHealth
    {
        [NonSerialized] private GameObject _healthBarInstance;
        [NonSerialized] private Slider _healthSlider;

        public void Initialize(Camera inCamera, GameObject healthBarPrefab)
        {
            if (healthBarPrefab != null)
            {
                // Instantiate health bar UI
                _healthBarInstance = Instantiate(healthBarPrefab);
                
                // Get UI components
                _healthSlider = _healthBarInstance.GetComponentInChildren<Slider>();
                Canvas canvas = _healthBarInstance.GetComponentInChildren<Canvas>();
                canvas.worldCamera = inCamera;
                
                RotateHealthBarByCamera(inCamera);
            }
        }

        void LateUpdate()
        {
            if (_healthBarInstance != null)
            {
                _healthBarInstance.transform.position = transform.position;
            }
        }

        private void RotateHealthBarByCamera(Camera inCamera)
        {
            if (_healthBarInstance != null)
            {
                _healthBarInstance.transform.rotation = inCamera.transform.rotation;
            }
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

        public override void ReceiveHealthDamage(int damage)
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