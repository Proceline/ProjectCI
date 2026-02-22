using ProjectCI.CoreSystem.Runtime.Passives;
using ProjectCI.CoreSystem.Runtime.Saving.Interfaces;

namespace IndAssets.Scripts.Passives.Relics
{
    public abstract class PvSoPassiveRelic : PvSoPassiveIndividual, IPvSaveEntry
    {
        public string EntryId => name;
    }
}