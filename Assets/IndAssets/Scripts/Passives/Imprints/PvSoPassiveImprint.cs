using IndAssets.Scripts.Units;
using ProjectCI.CoreSystem.Runtime.Passives;
using ProjectCI.CoreSystem.Runtime.Saving.Interfaces;
using UnityEngine;

namespace IndAssets.Scripts.Passives.Imprints
{
    public abstract class PvSoPassiveImprint : PvSoPassiveIndividual, IPvSaveEntry
    {
        public string EntryId => name;
        [SerializeField] protected EPvPersonalityName imprintType;

        public EPvPersonalityName ImprintType => imprintType;
    }
}