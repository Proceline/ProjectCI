using UnityEngine;

namespace IndAssets.Scripts.Dialogue.Data
{
    [CreateAssetMenu(fileName = "New Default DialogueSequence", menuName = "ProjectCI Dialogue/Default Dialogue Sequence")]
    public class PvSoDefaultDialogueEntry : PvSoDialogueEntry
    {

        [SerializeField]
        private string speakerName;

        [SerializeField]
        private Sprite portraitSprite;

        public override string SpeakerName => speakerName;
        public override Sprite PortraitSprite => portraitSprite;
    }
}