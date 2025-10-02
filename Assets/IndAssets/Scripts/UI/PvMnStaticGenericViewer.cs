using System;
using System.Collections.Generic;
using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.Utilities.Runtime.Events;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Runtime.GUI.Battle
{
    public class PvMnStaticGenericViewer : MonoBehaviour
    {
        [Serializable]
        public struct DamageTypeEnumColor
        {
            public PvEnDamageType type;
            public Color color;
        }
        
        [SerializeField] private PvSoSimpleDamageApplyEvent onDamageReceivedEvent;
        [SerializeField] private TextMeshPro defaultText;
        [SerializeField] private DamageTypeEnumColor[] damageTypeColors;

        private readonly Queue<TextMeshPro> _availableTexts = new();
        private readonly Dictionary<PvEnDamageType, Color> _damageTypeColorsDic = new();

        [SerializeField] private UnityEvent onInitializedRoot;

        public float normalTextSize = 10f;
        public float criticalTextSize = 12f;

        private void Start()
        {
            _availableTexts.Enqueue(defaultText);
            defaultText.gameObject.SetActive(false);
            onDamageReceivedEvent.RegisterCallback(ShowDamageValueText);
            onInitializedRoot?.Invoke();

            for (int i = 0; i < damageTypeColors.Length; i++)
            {
                _damageTypeColorsDic.TryAdd(damageTypeColors[i].type, damageTypeColors[i].color);
            }
        }

        private void OnDestroy()
        {
            onDamageReceivedEvent.UnregisterCallback(ShowDamageValueText);
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

        private void ShowDamageValueText(IEventOwner owner, DamageDescriptionParam damageParams)
        {
            var targetPosition = damageParams.Victim.transform.position + new Vector3(0, 3, 0);
            var showingValue = damageParams.FinalDamageBeforeAdjusted;
            var textMesh = GetAvailableDamageText();
            textMesh.transform.position = targetPosition;
            var damageType = (PvEnDamageType)damageParams.DamageType;
            if (_damageTypeColorsDic.TryGetValue(damageType, out var color))
            {
                textMesh.color = color;
            }

            var finalText = showingValue.ToString();
            if (damageParams.ContainsTag(UnitAbilityCoreExtensions.CriticalExtraInfoHint))
            {
                finalText += "!";
                textMesh.fontSize = criticalTextSize;
            }
            else if (damageParams.ContainsTag(UnitAbilityCoreExtensions.MissExtraInfoHint))
            {
                finalText = "Miss";
                textMesh.fontSize = normalTextSize;
            }
            else
            {
                textMesh.fontSize = normalTextSize;
            }

            textMesh.SetText(finalText);
            AnimateText(textMesh, 0.5f, targetPosition);
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
