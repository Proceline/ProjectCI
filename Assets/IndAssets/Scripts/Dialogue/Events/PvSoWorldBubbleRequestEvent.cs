using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;
using UnityEngine.Events;

namespace IndAssets.Scripts.Dialogue.Events
{
    /// <summary>
    /// ScriptableObject event channel for triggering a world-space speech bubble above a unit.
    ///
    /// Usage (any script, no direct manager reference needed):
    ///   bubbleRequestEvent.Raise(unit, "Ouch!", 2f);
    ///
    /// PvMnWorldDialogueBubble listens on this channel.
    /// </summary>
    [CreateAssetMenu(fileName = "WorldBubbleRequestEvent",
        menuName = "ProjectCI Dialogue/Events/World Bubble Request Event")]
    public class PvSoWorldBubbleRequestEvent : SoUnityEventBase
    {
        private readonly UnityEvent<PvMnBattleGeneralUnit, string, float> _onBubbleRequested = new();

        /// <summary>Broadcast a bubble request to all registered listeners.</summary>
        /// <param name="unit">The unit above whose head the bubble should appear.</param>
        /// <param name="text">Text to display.</param>
        /// <param name="duration">Seconds before auto-hiding. Pass 0 to keep until manually hidden.</param>
        public void Raise(PvMnBattleGeneralUnit unit, string text, float duration = 2f)
        {
            _onBubbleRequested.Invoke(unit, text, duration);
        }

        public void RegisterCallback(UnityAction<PvMnBattleGeneralUnit, string, float> callback)
        {
            _onBubbleRequested.AddListener(callback);
        }

        public void UnregisterCallback(UnityAction<PvMnBattleGeneralUnit, string, float> callback)
        {
            _onBubbleRequested.RemoveListener(callback);
        }
    }
}
