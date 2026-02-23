using System.Collections;
using IndAssets.Scripts.Dialogue.Data;
using IndAssets.Scripts.Dialogue.Events;
using IndAssets.Scripts.Dialogue.UI.ScreenSpace;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IndAssets.Scripts.Dialogue.Runtime
{
    /// <summary>
    /// Central dialogue manager that orchestrates screen-space dialogue sequences.
    /// 
    /// Scene setup:
    ///   1. Place this MonoBehaviour in the scene (e.g. on a DialogueManager GameObject).
    ///   2. Assign a PvSoDialogueRequestEvent SO asset → the manager subscribes automatically.
    ///   3. Assign the PvMnDialogueScreenPanel reference.
    ///   4. Assign two new InputActionReferences (Advance + ToggleAutoPlay).
    ///
    /// To trigger dialogue from any other script (no direct manager reference needed):
    ///   dialogueRequestEvent.Raise(mySequence);
    ///
    /// Or call directly if you hold a reference:
    ///   dialogueManager.PlaySequence(mySequence);
    /// </summary>
    public class PvMnDialogueManager : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────────────────
        [Header("Event Channels")]
        [Tooltip("Assign the shared DialogueRequestEvent SO. Any script can Raise() this to start a sequence.")]
        [SerializeField] private PvSoDialogueRequestEvent dialogueRequestEvent;

        [Header("UI Reference")]
        [SerializeField] private PvMnDialogueScreenPanel screenPanel;

        [Header("Input Actions  (create dedicated bindings in your Input Asset)")]
        [Tooltip("Action that advances / skips current dialogue line.")]
        [SerializeField] private InputActionReference advanceDialogueAction;

        [Tooltip("Action that toggles Auto-Play mode on/off.")]
        [SerializeField] private InputActionReference toggleAutoPlayAction;

        [Header("Settings")]
        [Tooltip("Characters revealed per second during typewriter.")]
        [SerializeField] private float typewriterSpeed = 30f;

        [Tooltip("Default seconds to wait before auto-advancing when an entry's AutoAdvanceDelay is 0.")]
        [SerializeField] private float defaultAutoAdvanceDelay = 1.5f;

        // ── State ───────────────────────────────────────────────────────────────
        private PvMnDialoguePlayer _player;
        private PvSoDialogueSequence _currentSequence;
        private int _currentEntryIndex;

        private bool _isPlaying;
        private bool _isAutoPlay;
        private bool _isTyping;

        private Coroutine _typewriterCoroutine;
        private Coroutine _autoAdvanceCoroutine;

        // ── Lifecycle ───────────────────────────────────────────────────────────
        private void Awake()
        {
            _player = new PvMnDialoguePlayer(typewriterSpeed);
        }

        private void Start()
        {
            if (dialogueRequestEvent)
                dialogueRequestEvent.RegisterCallback(PlaySequence);

            if (advanceDialogueAction)
            {
                advanceDialogueAction.action.Enable();
                advanceDialogueAction.action.canceled += OnAdvanceInput;
            }

            if (toggleAutoPlayAction)
            {
                toggleAutoPlayAction.action.Enable();
                toggleAutoPlayAction.action.canceled += OnToggleAutoPlay;
            }
        }

        private void OnDestroy()
        {
            if (dialogueRequestEvent)
                dialogueRequestEvent.UnregisterCallback(PlaySequence);

            if (advanceDialogueAction)
            {
                advanceDialogueAction.action.canceled -= OnAdvanceInput;
                advanceDialogueAction.action.Disable();
            }

            if (toggleAutoPlayAction)
            {
                toggleAutoPlayAction.action.canceled -= OnToggleAutoPlay;
                toggleAutoPlayAction.action.Disable();
            }
        }

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>
        /// Start playing a dialogue sequence. Safe to call from any script.
        /// If a sequence is already playing, it will be stopped first.
        /// </summary>
        public void PlaySequence(PvSoDialogueSequence sequence)
        {
            if (sequence == null || sequence.Entries == null || sequence.Entries.Length == 0)
            {
                Debug.LogWarning("[PvMnDialogueManager] PlaySequence called with null or empty sequence.");
                return;
            }

            StopAllDialogue();

            _currentSequence = sequence;
            _currentEntryIndex = 0;
            _isPlaying = true;
            _isAutoPlay = false;

            if (sequence.LockInputDuringPlay)
                PvMnGameController.IsControllerLocked = true;

            screenPanel.Show();
            screenPanel.SetAutoPlayIndicator(false);
            ShowCurrentEntry();
        }

        /// <summary>
        /// Immediately stop any running dialogue and hide the panel.
        /// </summary>
        public void StopAllDialogue()
        {
            StopTypewriterCoroutine();
            StopAutoAdvanceCoroutine();

            if (_currentSequence != null && _currentSequence.LockInputDuringPlay)
                PvMnGameController.IsControllerLocked = false;

            _isPlaying  = false;
            _isTyping   = false;
            _isAutoPlay = false;
            _currentSequence     = null;
            _currentEntryIndex   = 0;

            screenPanel.Hide();
        }

        // ── Internal flow ───────────────────────────────────────────────────────

        private void ShowCurrentEntry()
        {
            if (_currentEntryIndex >= _currentSequence.Entries.Length)
            {
                StopAllDialogue();
                return;
            }

            var entry = _currentSequence.Entries[_currentEntryIndex];
            screenPanel.SetupEntry(entry);

            _isTyping = true;
            _typewriterCoroutine = StartCoroutine(
                _player.PlayTypewriter(screenPanel.DialogueText, entry.DialogueText, OnTypewriterComplete)
            );
        }

        private void OnTypewriterComplete()
        {
            _isTyping = false;
            screenPanel.SetAdvancePrompt(true);

            if (!_isAutoPlay) return;

            var entry  = _currentSequence.Entries[_currentEntryIndex];
            float delay = entry.AutoAdvanceDelay > 0f ? entry.AutoAdvanceDelay : defaultAutoAdvanceDelay;
            _autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfterDelay(delay));
        }

        private IEnumerator AutoAdvanceAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            _autoAdvanceCoroutine = null;
            AdvanceToNext();
        }

        private void AdvanceToNext()
        {
            StopAutoAdvanceCoroutine();
            screenPanel.SetAdvancePrompt(false);
            _currentEntryIndex++;
            ShowCurrentEntry();
        }

        // ── Input handlers ──────────────────────────────────────────────────────

        private void OnAdvanceInput(InputAction.CallbackContext context)
        {
            if (!_isPlaying) return;

            if (_isTyping)
            {
                // First press: skip typewriter → show full text immediately.
                StopTypewriterCoroutine();
                _player.SkipToEnd(screenPanel.DialogueText);
                _isTyping = false;
                OnTypewriterComplete();
            }
            else
            {
                // Second press: advance to next entry.
                AdvanceToNext();
            }
        }

        private void OnToggleAutoPlay(InputAction.CallbackContext context)
        {
            if (!_isPlaying) return;

            _isAutoPlay = !_isAutoPlay;
            screenPanel.SetAutoPlayIndicator(_isAutoPlay);

            if (_isAutoPlay && !_isTyping)
            {
                // Already done typing on current entry: kick off auto-advance now.
                var entry   = _currentSequence.Entries[_currentEntryIndex];
                float delay = entry.AutoAdvanceDelay > 0f ? entry.AutoAdvanceDelay : defaultAutoAdvanceDelay;
                _autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfterDelay(delay));
            }
            else if (!_isAutoPlay)
            {
                // Turning Auto off: cancel any pending auto-advance.
                StopAutoAdvanceCoroutine();
            }
        }

        // ── Coroutine helpers ───────────────────────────────────────────────────

        private void StopTypewriterCoroutine()
        {
            if (_typewriterCoroutine == null) return;
            StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = null;
        }

        private void StopAutoAdvanceCoroutine()
        {
            if (_autoAdvanceCoroutine == null) return;
            StopCoroutine(_autoAdvanceCoroutine);
            _autoAdvanceCoroutine = null;
        }
    }
}
