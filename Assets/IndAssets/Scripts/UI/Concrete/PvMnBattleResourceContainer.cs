using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Components;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.Utilities.Runtime.Events;

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

        private static readonly Dictionary<string, Sprite> PreloadedTextures = new();

        public void Initialize(GridObject owner, Camera inCamera, PvMnBattleCamera cameraController, GameObject healthBarPrefab)
        {
            // Instantiate health bar UI
            _healthBarInstance = Instantiate(healthBarPrefab);

            // Get UI components
            _healthSlider = _healthBarInstance.GetComponentInChildren<Slider>();
            _statusLayoutGroup = _healthBarInstance.GetComponentInChildren<GridLayoutGroup>();
            Canvas canvas = _healthBarInstance.GetComponentInChildren<Canvas>();
            canvas.worldCamera = inCamera;

            RotateHealthBarByCamera(inCamera);
            cameraController.RegisterOnCameraRotationChanged(RotateHealthBarByCamera);

            _followingTarget = owner;
            
            FeLiteGameRules.XRaiserSimpleDamageApplyEvent.RegisterCallback(UpdateHealthViewInfo);
            _onUnitDyingEvent.RegisterCallback(OnObjectMarkedAsDead);
            
            _initialized = true;
        }

        private void OnDestroy()
        {
            if (_healthBarInstance != null)
            {
                Destroy(_healthBarInstance);
            }
            FeLiteGameRules.XRaiserSimpleDamageApplyEvent.UnregisterCallback(UpdateHealthViewInfo);
            _onUnitDyingEvent.UnregisterCallback(OnObjectMarkedAsDead);

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

        //private void UpdateStatusList(GridPawnUnit statusOwner, PvSoPassiveStatus statusType)
        //{
        //    if (statusOwner != _followingTarget) return;

        //    var currentStatusKey = statusType.GetType().Name;
        //    if (!PreloadedTextures.ContainsKey(currentStatusKey))
        //    {
        //        PreloadedTextures.Add(currentStatusKey, statusType.StatusIcon);
        //    }
            
        //    UpdateStatusListInternally(statusOwner);
        //}
        
        //private void UpdateStatusList(PvMnBattleGeneralUnit statusOwner)
        //{
        //    if (statusOwner != _followingTarget) return;
        //    UpdateStatusListInternally(statusOwner);
        //}
        
        //private void UpdateStatusListInternally(GridPawnUnit statusOwner)
        //{
        //    var statusList = statusOwner.GetStatusEffectContainer().GetStatusList();
        //    var prefab = _onStatusVisualApplyEvent.StatusViewPrefab;

        //    int index;
        //    for (var i = _statusLayoutGroup.transform.childCount - 1; i >= 0; i--)
        //    {
        //        Destroy(_statusLayoutGroup.transform.GetChild(i).gameObject);
        //    }
        //    _statusLayoutGroup.transform.DetachChildren();
            
        //    for (index = 0; index < statusList.Count; index++)
        //    {
        //        var image = Instantiate(prefab, _statusLayoutGroup.transform);
        //        if (PreloadedTextures.TryGetValue(statusList[index].StatusTag, out var statusIcon))
        //        {
        //            image.sprite = statusIcon;
        //        }
        //    }
        //}

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