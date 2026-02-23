using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace IndAssets.Scripts.Dialogue.Runtime
{
    /// <summary>
    /// Pure helper class (not a MonoBehaviour) that provides typewriter-style text reveal
    /// for a TMP_Text component. The owning MonoBehaviour is responsible for starting coroutines.
    /// </summary>
    public class PvMnDialoguePlayer
    {
        /// <summary>Characters revealed per second.</summary>
        private readonly float _charactersPerSecond;

        public PvMnDialoguePlayer(float charactersPerSecond = 30f)
        {
            _charactersPerSecond = Mathf.Max(1f, charactersPerSecond);
        }

        /// <summary>
        /// Coroutine that progressively reveals characters in <paramref name="textComponent"/>.
        /// Set full text on the component first, then call this.
        /// </summary>
        /// <param name="textComponent">The TMP_Text to animate.</param>
        /// <param name="fullText">Complete string to type out.</param>
        /// <param name="onComplete">Optional callback fired when the last character is shown.</param>
        public IEnumerator PlayTypewriter(TMP_Text textComponent, string fullText, Action onComplete = null)
        {
            // Assign the full text so TMP can build its mesh; then hide all characters.
            textComponent.text = fullText;
            textComponent.ForceMeshUpdate();

            int totalVisible = textComponent.textInfo.characterCount;
            textComponent.maxVisibleCharacters = 0;

            float charDelay = 1f / _charactersPerSecond;

            for (int visible = 0; visible <= totalVisible; visible++)
            {
                textComponent.maxVisibleCharacters = visible;
                yield return new WaitForSeconds(charDelay);
            }

            // Ensure all characters are definitely visible at the end.
            textComponent.maxVisibleCharacters = totalVisible;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Instantly reveals all characters (used when the player presses Skip during typewriter).
        /// </summary>
        public void SkipToEnd(TMP_Text textComponent)
        {
            // ForceMeshUpdate ensures characterCount is accurate before assigning.
            textComponent.ForceMeshUpdate();
            textComponent.maxVisibleCharacters = textComponent.textInfo.characterCount;
        }
    }
}
