using System;
using System.Collections;
using IndAssets.Scripts.Dialogue.Events;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using TMPro;
using UnityEngine;

namespace IndAssets.Scripts.Dialogue.UI.WorldSpace
{
    /// <summary>
    /// A world-space speech bubble that tracks a unit's position and always faces the main camera
    /// (billboard behaviour). Displays short text for a configurable duration then auto-hides.
    ///
    /// Scene setup (World Space Canvas child hierarchy suggested):
    ///
    ///   WorldDialogueBubble  ← this script, World Space Canvas root
    ///   └── BubbleRoot       ← parent of all visible bubble elements
    ///       ├── Background   ← Image (the speech bubble background)
    ///       ├── ArrowIndicator ← pointing-arrow Image (optional)
    ///       └── BubbleText   ← TMP_Text
    ///
    /// The Canvas Render Mode should be "World Space".
    /// Set an appropriate width/height and scale (e.g. 0.01 x 0.01 x 0.01).
    ///
    /// To trigger a bubble from any script (no direct reference needed):
    ///   bubbleRequestEvent.Raise(unit, "Ouch!", 2f);
    ///
    /// Or hold a direct reference and call:
    ///   bubble.ShowBubble(unit, "Ouch!", 2f);
    /// </summary>
    public class PvMnWorldDialogueBubble : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────────────────
        [Header("Event Channel")]
        [Tooltip("Assign the shared WorldBubbleRequestEvent SO. Optional: leave null if you will call ShowBubble() directly.")]
        [SerializeField] private PvSoWorldBubbleRequestEvent bubbleRequestEvent;

        [Header("UI References")]
        [Tooltip("Root object of all visual bubble elements. Toggle this to show/hide the bubble.")]
        [SerializeField] private GameObject bubbleRoot;

        [Tooltip("TMP_Text inside the bubble for displaying the spoken line.")]
        [SerializeField] private TMP_Text bubbleText;

        [Tooltip("Optional arrow or pointer indicator shown beneath the bubble.")]
        [SerializeField] private GameObject arrowIndicator;

        [Header("Positioning")]
        [Tooltip("Offset above the unit's pivot point (world units). Increase Y to clear tall models.")]
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);

        // ── State ───────────────────────────────────────────────────────────────
        [NonSerialized] private Transform _uiCameraTrans;
        private PvMnBattleGeneralUnit _trackedUnit;
        private Coroutine _hideCoroutine;

        private Transform GetCameraTrans
        {
            get
            {
                if (!_uiCameraTrans)
                {
                    var cameras = GameObject.FindGameObjectsWithTag("UICamera");
                    if (cameras.Length > 0)
                    {
                        _uiCameraTrans = cameras[0].transform;
                    }
                }

                return _uiCameraTrans;
            }
        }

        // ── Lifecycle ───────────────────────────────────────────────────────────
        private void Awake()
        {
            HideBubbleImmediate();
        }

        private void Start()
        {
            if (bubbleRequestEvent)
                bubbleRequestEvent.RegisterCallback(OnBubbleRequested);
        }

        private void OnDestroy()
        {
            if (bubbleRequestEvent)
                bubbleRequestEvent.UnregisterCallback(OnBubbleRequested);
        }

        /// <summary>
        /// LateUpdate keeps the bubble glued to the tracked unit and facing the camera.
        /// Using LateUpdate ensures unit movement has already been applied this frame.
        /// </summary>
        private void LateUpdate()
        {
            if (!bubbleRoot || !bubbleRoot.activeSelf || !_trackedUnit) return;

            // Position: follow unit head
            transform.position = _trackedUnit.transform.position + worldOffset;

            // Billboard: always face the main camera
            if (GetCameraTrans)
                transform.rotation = GetCameraTrans.rotation;
        }

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>
        /// Show a speech bubble above the given unit for <paramref name="duration"/> seconds.
        /// If a bubble is already showing, it is replaced immediately.
        /// </summary>
        /// <param name="unit">Unit whose head anchors the bubble.</param>
        /// <param name="text">Line of text to display.</param>
        /// <param name="duration">Seconds before auto-hiding. Pass 0 to keep indefinitely.</param>
        public void ShowBubble(PvMnBattleGeneralUnit unit, string text, float duration = 2f)
        {
            StopHideCoroutine();

            _trackedUnit = unit;

            if (bubbleText)
                bubbleText.text = text;

            if (arrowIndicator)
                arrowIndicator.SetActive(true);

            bubbleRoot.SetActive(true);

            if (duration > 0f)
                _hideCoroutine = StartCoroutine(HideAfterDelay(duration));
        }

        /// <summary>
        /// Immediately hide the bubble and stop tracking.
        /// </summary>
        public void HideBubble()
        {
            StopHideCoroutine();
            HideBubbleImmediate();
        }

        // ── Private helpers ─────────────────────────────────────────────────────

        private void HideBubbleImmediate()
        {
            _trackedUnit = null;
            if (bubbleRoot)
                bubbleRoot.SetActive(false);
        }

        private void OnBubbleRequested(PvMnBattleGeneralUnit unit, string text, float duration)
        {
            ShowBubble(unit, text, duration);
        }

        private IEnumerator HideAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            HideBubbleImmediate();
            _hideCoroutine = null;
        }

        private void StopHideCoroutine()
        {
            if (_hideCoroutine == null) return;
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }
    }
}
