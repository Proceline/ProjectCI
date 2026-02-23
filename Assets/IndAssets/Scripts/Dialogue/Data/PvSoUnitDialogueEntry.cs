using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;

namespace IndAssets.Scripts.Dialogue.Data
{
    [CreateAssetMenu(fileName = "New Unit DialogueSequence", menuName = "ProjectCI Dialogue/Unit Dialogue Sequence")]
    public class PvSoUnitDialogueEntry : PvSoDialogueEntry
    {
        [SerializeField]
        private PvSoBattleUnitData talkingUnit;

        public override string SpeakerName => talkingUnit.GetCharacterName();
        public override Sprite PortraitSprite => talkingUnit.GetIcon;
    }
}