using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Components;
using UnityEngine;
using UnityEngine.UI;
using System;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.Utilities.Runtime.Events;
using ProjectCI.CoreSystem.Runtime.Passives;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.Runtime.GUI.Battle
{
    [StaticInjectableTarget]
    public class PvMnBattleResourceContainer : BattleHealth
    {
        [NonSerialized] private GameObject _healthBarInstance;
        [NonSerialized] private Slider _healthSlider;
        [NonSerialized] private GridLayoutGroup _statusLayoutGroup;
        [NonSerialized] private bool _initialized;
        private GridObject _followingTarget;

        [Inject] private static ITargetUnitDeathEvent _onUnitDyingEvent;
        [Inject] private static IOnStatusApplyEvent _onStatusApplyEvent;

        [NonSerialized] private Image[] _holdingPassiveList;
        [NonSerialized] private string[] _holdingPassiveNamesList;

        public void Initialize(GridObject owner, Camera inCamera, PvMnBattleCamera cameraController, GameObject healthBarPrefab)
        {
            // Instantiate health bar UI
            _healthBarInstance = Instantiate(healthBarPrefab);

            // Get UI components
            _healthSlider = _healthBarInstance.GetComponentInChildren<Slider>();
            _statusLayoutGroup = _healthBarInstance.GetComponentInChildren<GridLayoutGroup>();
            _holdingPassiveList = _statusLayoutGroup.GetComponentsInChildren<Image>();
            Canvas canvas = _healthBarInstance.GetComponentInChildren<Canvas>();
            canvas.worldCamera = inCamera;

            RotateHealthBarByCamera(inCamera);
            cameraController.RegisterOnCameraRotationChanged(RotateHealthBarByCamera);

            _followingTarget = owner;
            
            FeLiteGameRules.XRaiserSimpleDamageApplyEvent.RegisterCallback(UpdateHealthViewInfo);
            _onUnitDyingEvent.RegisterCallback(OnObjectMarkedAsDead);
            
            _initialized = true;

            _holdingPassiveNamesList = new string[_holdingPassiveList.Length];
            foreach (var passiveImage in _holdingPassiveList)
            {
                passiveImage.gameObject.SetActive(false);
            }

            _onStatusApplyEvent.RegisterVisualCallback(OnStatusApplied);
            _onStatusApplyEvent.RegisterUnsetVisualCallback(OnStatusDisposed);
        }

        private void OnDestroy()
        {
            if (_healthBarInstance != null)
            {
                Destroy(_healthBarInstance);
            }
            FeLiteGameRules.XRaiserSimpleDamageApplyEvent.UnregisterCallback(UpdateHealthViewInfo);
            _onUnitDyingEvent.UnregisterCallback(OnObjectMarkedAsDead);

            _onStatusApplyEvent.UnregisterVisualCallback(OnStatusApplied);
            _onStatusApplyEvent.UnregisterUnsetVisualCallback(OnStatusDisposed);
            _initialized = false;
        }

        private void OnObjectMarkedAsDead(PvMnBattleGeneralUnit deadUnit)
        {
            if (deadUnit != _followingTarget) return;
            _healthBarInstance.SetActive(false);
        }

        private void UpdateHealthViewInfo(IEventOwner owner, DamageDescriptionParam damageParams)
        {
            if (_followingTarget != damageParams.Victim) return;
            SetHealth(damageParams.AfterValue);
        }

        private void OnStatusApplied(GridPawnUnit statusOwner, PvSoPassiveBase passive)
        {
            if (statusOwner != _followingTarget) return;

            for (int i = 0; i < _holdingPassiveList.Length; i++)
            {
                var passiveImage = _holdingPassiveList[i];
                if (!passiveImage.gameObject.activeSelf)
                {
                    passiveImage.gameObject.SetActive(true);
                    passiveImage.sprite = passive.Icon;
                    _holdingPassiveNamesList[i] = passive.name;
                    return;
                }
            }
        }

        private void OnStatusDisposed(GridPawnUnit statusOwner, PvSoPassiveBase passive)
        {
            if (statusOwner != _followingTarget) return;

            for (int i = 0; i < _holdingPassiveNamesList.Length; i++)
            {
                if (_holdingPassiveNamesList[i] == passive.name)
                {
                    var passiveImage = _holdingPassiveList[i];
                    passiveImage.gameObject.SetActive(false);
                    return;
                }
            }
        }

        private void LateUpdate()
        {
            if (_initialized)
            {
                _healthBarInstance.transform.position = transform.position;
            }
        }

        private void RotateHealthBarByCamera(Camera uiCamera)
        {
            if (!_healthBarInstance) return;
            _healthBarInstance.transform.rotation = uiCamera.transform.rotation;
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
    }
} 