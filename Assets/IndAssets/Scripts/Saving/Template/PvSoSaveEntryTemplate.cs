using ProjectCI.CoreSystem.Runtime.Saving.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Saving.Data
{
    /// <summary>
    /// Mock ScriptableObject of PvSaveData
    /// </summary>
    [CreateAssetMenu(fileName = "PvSoSaveEntry", menuName = "ProjectCI Tools/Save/PvSoSaveEntry")]
    public class PvSoSaveEntryTemplate : ScriptableObject, IPvSaveEntry
    {
        [SerializeField] private string saveDataName;
        [SerializeField] private string customEntryId;

        public string EntryId => customEntryId;

        [SerializeField] private List<PvSoSaveCharacterTemplate> characters = new();

        public PvSaveData TranslateToSaveData()
        {
            var saveData = new PvSaveData();
            foreach (var character in characters)
            {
                var characterData = character.TranslateToCharacter(out var weaponInstance);
                saveData.AddUnlockedCharacter(characterData.CharacterId);
                saveData.SetCharacterData(characterData);
                if (weaponInstance != null)
                {
                    saveData.AddWeaponInstance(weaponInstance);
                }
            }

            return saveData;
        }
    }
}