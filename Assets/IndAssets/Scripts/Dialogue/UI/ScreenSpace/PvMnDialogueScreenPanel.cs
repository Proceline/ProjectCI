using IndAssets.Scripts.Dialogue.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IndAssets.Scripts.Dialogue.UI.ScreenSpace
{
    /// <summary>
    /// Controls the screen-space dialogue panel: two portrait slots, a dialogue text box,
    /// a speaker name label, an auto-play indicator, and an advance prompt.
    ///
    /// Scene setup (Prefab / Canvas hierarchy suggested):
    ///
    ///   DialogueScreenPanel (Canvas or child Panel)
    ///   ├── PortraitLeft          ← Image + CanvasGroup
    ///   ├── PortraitRight         ← Image + CanvasGroup
    ///   ├── DialogueBox
    ///   │   ├── SpeakerNameText   ← TMP_Text
    ///   │   ├── DialogueText      ← TMP_Text  (assign to dialogueText)
    ///   │   ├── AdvancePrompt     ← any GameObject (e.g. "▶" icon)
    ///   │   └── AutoPlayLabel     ← any GameObject (e.g. "[AUTO]" text)
    ///   └── (PanelRoot CanvasGroup for fade-in/out)
    ///
    /// The manager calls SetupEntry() per dialogue line and reads the DialogueText
    /// property to drive the typewriter.
    /// </summary>
    public class PvMnDialogueScreenPanel : MonoBehaviour
    {
        // ── Portrait slots ──────────────────────────────────────────────────────
        [Header("Portrait – Left")]
        [SerializeField] private Image leftPortraitImage;
        [SerializeField] private CanvasGroup leftPortraitGroup;

        [Header("Portrait – Right")]
        [SerializeField] private Image rightPortraitImage;
        [SerializeField] private CanvasGroup rightPortraitGroup;

        // ── Text ────────────────────────────────────────────────────────────────
        [Header("Dialogue Text")]
        [SerializeField] private TMP_Text speakerNameText;
        [SerializeField] private TMP_Text dialogueText;

        // ── HUD elements ────────────────────────────────────────────────────────
        [Header("HUD Indicators")]
        [Tooltip("Object shown while Auto-Play is active (e.g. 'AUTO' label).")]
        [SerializeField] private GameObject autoPlayIndicator;

        [Tooltip("Object shown when the typewriter has finished and waiting for player input.")]
        [SerializeField] private GameObject advancePrompt;

        [Header("Settings")]
        [Tooltip("Alpha applied to the non-speaking portrait to create a dimming effect.")]
        [Range(0f, 1f)]
        [SerializeField] private float dimmedAlpha = 0.4f;

        // ── Public accessor used by PvMnDialogueManager ─────────────────────────
        public TMP_Text DialogueText => dialogueText;

        // ── Panel visibility ────────────────────────────────────────────────────

        /// <summary>Activate and fully show the panel.</summary>
        public void Show()
        {
            gameObject.SetActive(true);
            SetAdvancePrompt(false);
            SetAutoPlayIndicator(false);

            DisablePortraitGroups();
        }

        /// <summary>Deactivate the panel entirely.</summary>
        public void Hide()
        {
            gameObject.SetActive(false);

            DisablePortraitGroups();
        }

        private void DisablePortraitGroups()
        {
            leftPortraitGroup.gameObject.SetActive(false);
            rightPortraitGroup.gameObject.SetActive(false);
        }

        // ── Entry display ───────────────────────────────────────────────────────

        /// <summary>
        /// Populate the panel with one dialogue entry.
        /// The manager calls this before starting the typewriter for that entry.
        /// </summary>
        public void SetupEntry(PvSoDialogueEntry entry)
        {
            // Speaker name
            if (speakerNameText)
                speakerNameText.text = entry.SpeakerName;

            // Clear dialogue text immediately (typewriter will fill it in)
            if (dialogueText)
                dialogueText.text = string.Empty;

            // Portraits
            ApplyPortraitForSide(entry.PortraitSide, entry.PortraitSprite);

            // Hide advance prompt while new line loads
            SetAdvancePrompt(false);
        }

        // ── Indicator helpers ───────────────────────────────────────────────────

        /// <summary>Show/hide the Auto-Play indicator.</summary>
        public void SetAutoPlayIndicator(bool isAutoPlay)
        {
            if (autoPlayIndicator)
                autoPlayIndicator.SetActive(isAutoPlay);
        }

        /// <summary>Show/hide the "press to advance" prompt.</summary>
        public void SetAdvancePrompt(bool visible)
        {
            if (advancePrompt)
                advancePrompt.SetActive(visible);
        }

        // ── Private helpers ─────────────────────────────────────────────────────

        private void ApplyPortraitForSide(PvDialoguePortraitSide side, Sprite sprite)
        {
            switch (side)
            {
                case PvDialoguePortraitSide.Left:
                    AssignSprite(leftPortraitImage, sprite);
                    SetPortraitGroupAlpha(leftPortraitGroup,  1f);
                    SetPortraitGroupAlpha(rightPortraitGroup, dimmedAlpha);
                    break;

                case PvDialoguePortraitSide.Right:
                    AssignSprite(rightPortraitImage, sprite);
                    SetPortraitGroupAlpha(rightPortraitGroup, 1f);
                    SetPortraitGroupAlpha(leftPortraitGroup,  dimmedAlpha);
                    break;

                case PvDialoguePortraitSide.None:
                    // Narrator / no portrait: dim both
                    SetPortraitGroupAlpha(leftPortraitGroup,  dimmedAlpha);
                    SetPortraitGroupAlpha(rightPortraitGroup, dimmedAlpha);
                    break;
            }
        }

        private static void AssignSprite(Image portraitImage, Sprite sprite)
        {
            if (!portraitImage) return;

            if (sprite)
            {
                portraitImage.sprite = sprite;
                portraitImage.gameObject.SetActive(true);
            }
            else
            {
                portraitImage.gameObject.SetActive(false);
            }
        }

        private static void SetPortraitGroupAlpha(CanvasGroup group, float alpha)
        {
            if (!group.gameObject.activeSelf && alpha > 0.99f)
            {
                group.gameObject.SetActive(true);
            }

            if (group) group.alpha = alpha;
        }
    }
}
