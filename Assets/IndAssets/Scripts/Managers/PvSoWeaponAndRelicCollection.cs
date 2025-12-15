using System.Collections.Generic;
using IndAssets.Scripts.Passives.Relics;
using IndAssets.Scripts.Weapons;
using UnityEngine;

namespace IndAssets.Scripts.Managers
{
    /// <summary>
    /// Collection manager for all Weapon and Relic ScriptableObject assets in the project
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponAndRelicCollection", menuName = "ProjectCI Tools/Weapon and Relic Collection")]
    public class PvSoWeaponAndRelicCollection : ScriptableObject
    {
        [Header("Weapons")]
        [SerializeField] private List<PvSoWeaponData> weapons = new List<PvSoWeaponData>();
        
        [Header("Relics")]
        [SerializeField] private List<PvSoPassiveRelic> relics = new List<PvSoPassiveRelic>();

        private readonly Dictionary<string, PvSoWeaponData> _weaponsDict = new();
        private readonly Dictionary<string, PvSoPassiveRelic> _relicsDict = new();

        public List<PvSoWeaponData> Weapons => weapons;
        public List<PvSoPassiveRelic> Relics => relics;

        public Dictionary<string, PvSoWeaponData> WeaponsDict
        {
            get
            {
                if (_weaponsDict.Count == weapons.Count) return _weaponsDict;
                _weaponsDict.Clear();
                foreach (var weaponData in weapons)
                {
                    _weaponsDict.Add(weaponData.EntryId, weaponData);
                }

                return _weaponsDict;
            }
        }

        public Dictionary<string, PvSoPassiveRelic> RelicsDict
        {
            get
            {
                if (_relicsDict.Count == relics.Count) return _relicsDict;
                _relicsDict.Clear();
                foreach (var relicData in relics)
                {
                    _relicsDict.Add(relicData.EntryId, relicData);
                }

                return _relicsDict;
            }
        }

        public PvSoWeaponData GetWeaponData(string entryId) => WeaponsDict.GetValueOrDefault(entryId);
        public PvSoPassiveRelic GetRelicData(string entryId) => RelicsDict.GetValueOrDefault(entryId);
    }
}
