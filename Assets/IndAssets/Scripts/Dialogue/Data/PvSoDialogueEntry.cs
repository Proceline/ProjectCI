using UnityEngine;

namespace IndAssets.Scripts.Dialogue.Data
{
    public enum PvDialoguePortraitSide
    {
        Left,
        Right,
        None
    }

    /// <summary>
    /// A single line/entry in a dialogue sequence.
    /// Configure speaker name, portrait sprite, text, which side the portrait appears on,
    /// and how long to wait before auto-advancing (0 = manual only).
    /// </summary>
    public abstract class PvSoDialogueEntry : ScriptableObject
    {
        [TextArea(2, 6)]
        [SerializeField]
        private string dialogueText;

        [SerializeField]
        private PvDialoguePortraitSide portraitSide = PvDialoguePortraitSide.Left;

        /// <summary>
        /// Seconds to wait before auto-advancing to the next entry when in Auto mode.
        /// Set to 0 to use the manager's default fallback delay.
        /// </summary>
        [Tooltip("Seconds to wait after typewriter finishes before auto-advancing. Use 0 to fall back to manager default.")]
        [SerializeField]
        private float autoAdvanceDelay = 0f;

        public abstract string SpeakerName { get; }
        public abstract Sprite PortraitSprite { get; }
        public string DialogueText => dialogueText;
        public PvDialoguePortraitSide PortraitSide => portraitSide;
        public float AutoAdvanceDelay => autoAdvanceDelay;
    }
}
