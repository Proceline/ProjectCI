using System;
using System.Collections.Generic;
using ProjectCI.Utilities.Runtime.Events;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Runtime.GUI.Battle
{
    public class PvMnStaticGenericViewer : MonoBehaviour
    {
        [SerializeField] private PvSoSimpleDamageApplyEvent onDamageReceivedEvent;
        [SerializeField] private TextMeshPro defaultText;

        [NonSerialized] private readonly Queue<TextMeshPro> _availableTexts = new();

        [SerializeField] private UnityEvent onInitializedRoot;

        private void Start()
        {
            _availableTexts.Enqueue(defaultText);
            defaultText.gameObject.SetActive(false);
            onDamageReceivedEvent.RegisterCallback(ShowDamageValueText);
            onInitializedRoot?.Invoke();
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
            textMesh.SetText(showingValue.ToString());
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
