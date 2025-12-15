using ProjectCI.CoreSystem.Runtime.Passives;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.Saving.Interfaces;
using UnityEngine;

namespace IndAssets.Scripts.Passives.Relics
{
    public class PvSoPassiveRelic : PvSoPassiveBase, IPvSaveEntry
    {
        public string EntryId => name;

        protected override void InstallPassiveInternally(GridPawnUnit unit)
        {
            
        }

        protected override void DisposePassiveInternally(GridPawnUnit unit)
        {
            
        }
    }
}