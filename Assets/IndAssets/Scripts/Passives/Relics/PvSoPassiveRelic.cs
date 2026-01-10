using ProjectCI.CoreSystem.Runtime.Passives;
using ProjectCI.CoreSystem.Runtime.Saving.Interfaces;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;

namespace IndAssets.Scripts.Passives.Relics
{
    public class PvSoPassiveRelic : PvSoPassiveIndividual, IPvSaveEntry
    {
        public string EntryId => name;

        protected override void InstallPassiveInternally(PvMnBattleGeneralUnit unit)
        {
            
        }

        protected override void DisposePassiveInternally(PvMnBattleGeneralUnit unit)
        {
            
        }
    }
}