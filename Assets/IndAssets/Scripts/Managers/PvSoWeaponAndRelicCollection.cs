using System.Collections.Generic;
using IndAssets.Scripts.Passives.Relics;
using IndAssets.Scripts.Weapons;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;

namespace IndAssets.Scripts.Managers
{
    /// <summary>
    /// Collection manager for all Weapon and Relic ScriptableObject assets in the project
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponAndRelicCollection", menuName = "ProjectCI Tools/Weapon and Relic Collection")]
    public class PvSoWeaponAndRelicCollection : ScriptableObject
    {
        #if UNITY_EDITOR
        public static string UnitsPropertyName => nameof(availableUnits);
        public static string BodyMeshPrefabsPropertyName => nameof(bodyMeshPrefabs);
        #endif
        
        [SerializeField] private List<PvSoBattleUnitData> availableUnits = new();
        
        [Header("Weapons")]
        [SerializeField] private List<PvSoWeaponData> weapons = new List<PvSoWeaponData>();
        
        [Header("Relics")]
        [SerializeField] private List<PvSoPassiveRelic> relics = new List<PvSoPassiveRelic>();

        [Header("Body Mesh Prefabs")]
        [SerializeField]
        private List<PvMnMeshPartController> bodyMeshPrefabs = new List<PvMnMeshPartController>();

        private readonly Dictionary<string, PvSoBattleUnitData> _availableUnitsDict = new();
        private readonly Dictionary<string, PvSoWeaponData> _weaponsDict = new();
        private readonly Dictionary<string, PvSoPassiveRelic> _relicsDict = new();

        public List<PvSoWeaponData> Weapons => weapons;
        public List<PvSoPassiveRelic> Relics => relics;

        public Dictionary<string, PvSoBattleUnitData> UnitDataDict
        {
            get
            {
                if (_availableUnitsDict.Count == availableUnits.Count) return _availableUnitsDict;
                _availableUnitsDict.Clear();
                foreach (var unitData in availableUnits)
                {
                    _availableUnitsDict.Add(unitData.EntryId, unitData);
                }

                return _availableUnitsDict;
            }
        }
        
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

        public PvMnMeshPartController GetBodyMeshPrefab(string bodyTypeName)
        {
            var prefab = bodyMeshPrefabs.Find(p => p.name == bodyTypeName);
            if (prefab == null)
            {
                Debug.LogError($"Body mesh prefab not found for {bodyTypeName}");
                return bodyMeshPrefabs[0];
            }

            return prefab;
        }

        public PvMnMeshPartController GetDefaultBodyMeshPrefab()
        {
            return bodyMeshPrefabs[0];
        }
    }
}
