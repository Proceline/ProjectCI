using IndAssets.Scripts.Weapons;
using ProjectCI.CoreSystem.Runtime.Saving.Interfaces;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Saving.Data
{
    /// <summary>
    /// Mock ScriptableObject of PvCharacterSaveData
    /// </summary>
    [CreateAssetMenu(fileName = "PvSoSaveCharacter", menuName = "ProjectCI Tools/Save/PvSoSaveCharacter")]
    public class PvSoSaveCharacterTemplate : ScriptableObject, IPvSaveEntry
    {
        [SerializeField] private PvSoBattleUnitData unitData;
        [SerializeField] private PvSoWeaponData holdingWeapon;
        [SerializeField] private string holdingWeaponInstId;

        public string EntryId => unitData.EntryId;

        public PvCharacterSaveData TranslateToCharacter(out PvWeaponInstance weaponInstance)
        {
            var targetCharacterSaveData = new PvCharacterSaveData(unitData);
            weaponInstance = holdingWeapon ? TranslateHoldingWeapon() : null;
            targetCharacterSaveData.WeaponInstanceId = holdingWeaponInstId;
            return targetCharacterSaveData;
        }

        public PvWeaponInstance TranslateHoldingWeapon()
        {
            var weaponInstance = new PvWeaponInstance(holdingWeapon);
            weaponInstance.InstanceId = holdingWeaponInstId;
            weaponInstance.IsEquipped = true;
            weaponInstance.EquippedToCharacterId = EntryId;
            return weaponInstance;
        }

    }
}