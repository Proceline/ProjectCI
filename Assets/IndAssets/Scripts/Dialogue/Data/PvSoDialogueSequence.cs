using UnityEngine;

namespace IndAssets.Scripts.Dialogue.Data
{
    /// <summary>
    /// An ordered collection of PvSoDialogueEntry assets that form a complete dialogue scene.
    /// Assign entries in Inspector order; the manager will play them top-to-bottom.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogueSequence", menuName = "ProjectCI Dialogue/Dialogue Sequence")]
    public class PvSoDialogueSequence : ScriptableObject
    {
        [Tooltip("Unique identifier for this sequence (used for save-state queries).")]
        [SerializeField]
        private string sequenceId;

        [SerializeField]
        private PvSoDialogueEntry[] entries;

        /// <summary>
        /// When true, sets PvMnGameController.IsControllerLocked = true for the duration
        /// of this sequence, then restores it on completion or skip.
        /// </summary>
        [Tooltip("Lock battle input while this sequence is playing.")]
        [SerializeField]
        private bool lockInputDuringPlay = true;

        public string SequenceId         => sequenceId;
        public PvSoDialogueEntry[] Entries => entries;
        public bool LockInputDuringPlay  => lockInputDuringPlay;
    }
}
