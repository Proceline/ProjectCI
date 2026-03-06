using IndAssets.Scripts.Abilities;
using IndAssets.Scripts.TacticTool;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.Utilities.Runtime.Events;
using ProjectCI.Utilities.Runtime.Pools;
using ProjectCI_Animation.Runtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Runtime.GUI.Battle
{
    [StaticInjectableTarget]
    public class PvMnStaticGenericViewer : MonoBehaviour
    {
        [Serializable]
        public struct DamageTypeEnumColor
        {
            public PvEnDamageType type;
            public Color color;
        }
        
        [SerializeField] private TextMeshPro defaultText;
        [SerializeField] private DamageTypeEnumColor[] damageTypeColors;

        private readonly Queue<TextMeshPro> _availableTexts = new();
        private readonly Dictionary<PvEnDamageType, Color> _damageTypeColorsDic = new();

        [SerializeField] private UnityEvent onInitializedRoot;
        [SerializeField] private PvSoUnitsDictionary unitsDictionary;

        [Inject] private static readonly IEnergyUpdateEvent OnEnergyUpdateEvent;
        [Inject] private static readonly ITargetUnitPreRestEvent OnUnitPreRestEvent;
        [Inject] private static readonly ITargetUnitPostRestEvent OnUnitPostRestEvent;
        [Inject] private static readonly IPvDamageLikeApplyEvent OnDamageApplyEvent;

        [SerializeField] private GameObject normalHitEffectPrefab, blockHitEffectPrefab;

        public float smallTextSize = 8f;
        public float normalTextSize = 10f;
        public float criticalTextSize = 12f;

        private void Start()
        {
            _availableTexts.Enqueue(defaultText);
            defaultText.gameObject.SetActive(false);
            OnDamageApplyEvent.RegisterCallbackVisually(ShowDamageValueVisually);
            OnUnitPreRestEvent.RegisterCallback(EnableEnergyHint);
            OnUnitPostRestEvent.RegisterCallback(DisableEnergyHint);

            onInitializedRoot?.Invoke();

            for (int i = 0; i < damageTypeColors.Length; i++)
            {
                _damageTypeColorsDic.TryAdd(damageTypeColors[i].type, damageTypeColors[i].color);
            }
        }

        private void OnDestroy()
        {
            OnUnitPreRestEvent.UnregisterCallback(EnableEnergyHint);
            OnUnitPostRestEvent.UnregisterCallback(DisableEnergyHint);
            OnDamageApplyEvent.UnregisterCallbackVisually(ShowDamageValueVisually);
        }

        private TextMeshPro GetAvailableDamageText()
        {
            if (_availableTexts.TryDequeue(out var result))
            {
                result.gameObject.SetActive(true);
                result.SetText(string.Empty);
                return result;
            }

            var newText = Instantiate(defaultText, defaultText.transform.parent);
            newText.SetText(string.Empty);
            return newText;
        }

        /// <summary>
        /// Including hit effects
        /// </summary>
        /// <param name="sourceInfo"></param>
        /// <param name="valueInfo"></param>
        /// <param name="typeInfo"></param>
        private void ShowDamageValueVisually(PvEventDamageSourceInfo sourceInfo, PvEventDamageValueInfo valueInfo, PvEventDamageTypeInfo typeInfo)
        {
            if (!unitsDictionary.TryGetValue(sourceInfo.ToId, out var victim))
            {
                return;
            }

            var targetPosition = victim.transform.position + new Vector3(0, 3, 0);
            var effectPosition = victim.transform.position + Vector3.up * 2;
            var showingValue = valueInfo.DeltaValue;
            var textMesh = GetAvailableDamageText();
            textMesh.transform.position = targetPosition;

            if (_damageTypeColorsDic.TryGetValue(typeInfo.Type, out var color))
            {
                textMesh.color = color;
            }

            var finalText = showingValue.ToString();

            var damageReaction = typeInfo.Reaction;
            if (damageReaction == PvEnDamageReact.MissHit)
            {
                finalText = "Miss";
                textMesh.fontSize = normalTextSize;
            }
            else if (damageReaction.HasFlag(PvEnDamageReact.Critical))
            {
                finalText += "!";
                textMesh.fontSize = criticalTextSize;
            }
            else
            {
                textMesh.fontSize = normalTextSize;
            }

            if (damageReaction.HasFlag(PvEnDamageReact.ActualHit))
            {
                var hitEffect = MnObjectPool.Instance.Get(normalHitEffectPrefab);
                hitEffect.transform.position = effectPosition;
            }
            else if (damageReaction.HasFlag(PvEnDamageReact.Blocked))
            {
                var hitEffect = MnObjectPool.Instance.Get(blockHitEffectPrefab);
                hitEffect.transform.position = effectPosition;
            }
            
            textMesh.SetText(finalText);
            AnimateText(textMesh, 0.5f, targetPosition);
        }

        private void EnableEnergyHint(PvMnBattleGeneralUnit battleUnit)
        {
            OnEnergyUpdateEvent.RegisterCallbackVisually(ShowEnergyText);
        }

        private void DisableEnergyHint(PvMnBattleGeneralUnit battleUnit)
        {
            OnEnergyUpdateEvent.UnregisterCallbackVisually(ShowEnergyText);
        }

        private void ShowEnergyText(string ownerId, int[] energyAdjustment)
        {
            if (unitsDictionary.TryGetValue(ownerId, out var unit))
            {
                var targetPosition = unit.Position + new Vector3(0, 3, 0);
                var deltaValue = energyAdjustment[1] - energyAdjustment[0];

                if (deltaValue != 0)
                {
                    var deltaValueString = deltaValue > 0 ? $"+{deltaValue}" : deltaValue.ToString(); 
                    
                    var textMesh = GetAvailableDamageText();
                    textMesh.transform.position = targetPosition;
                    textMesh.color = Color.blue;
                    textMesh.fontSize = smallTextSize;
                    textMesh.SetText(deltaValueString);
                    AnimateText(textMesh, 0.5f, targetPosition);
                }
            }
        }

        private async void AnimateText(TextMeshPro text, float duration, Vector3 startPosition)
        {
            float elapsed = 0;
            var finalPosition = startPosition + new Vector3(0, 3, 0);
            Color originalColor = text.color;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var progress = Mathf.Clamp01(elapsed / duration);
                var currentPosition = Vector3.Lerp(startPosition, finalPosition, progress);
                text.transform.position = currentPosition;
                originalColor.a = 1f - progress;
                text.color = originalColor;
                await Awaitable.EndOfFrameAsync();
            }
            text.gameObject.SetActive(false);
            _availableTexts.Enqueue(text);
        }
        
        public void SetupCorrectRotation(Camera targetCamera)
        {
            transform.rotation = targetCamera.transform.rotation;
        }
    }
}
