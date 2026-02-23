using IndAssets.Scripts.Dialogue.Data;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;
using UnityEngine.Events;

namespace IndAssets.Scripts.Dialogue.Events
{
    /// <summary>
    /// ScriptableObject event channel for triggering a full screen-space dialogue sequence.
    /// 
    /// Usage (any script, no direct reference to the manager needed):
    ///   dialogueRequestEvent.Raise(mySequence);
    ///
    /// The PvMnDialogueManager listens on this channel and handles playback.
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueRequestEvent",
        menuName = "ProjectCI Dialogue/Events/Dialogue Request Event")]
    public class PvSoDialogueRequestEvent : SoUnityEventBase
    {
        private readonly UnityEvent<PvSoDialogueSequence> _onDialogueRequested = new();

        /// <summary>Broadcast a dialogue sequence to all registered listeners.</summary>
        public void Raise(PvSoDialogueSequence sequence)
        {
            _onDialogueRequested.Invoke(sequence);
        }

        public void RegisterCallback(UnityAction<PvSoDialogueSequence> callback)
        {
            _onDialogueRequested.AddListener(callback);
        }

        public void UnregisterCallback(UnityAction<PvSoDialogueSequence> callback)
        {
            _onDialogueRequested.RemoveListener(callback);
        }
    }
}
