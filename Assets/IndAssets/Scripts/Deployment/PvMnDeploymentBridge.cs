using Assets.IndAssets.Scripts.Deployment;
using IndAssets.Scripts.Managers;
using ProjectCI.CoreSystem.Runtime.Saving;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.Deployment
{
    public class PvMnDeploymentBridge : MonoBehaviour
    {
        private void Awake()
        {
            onBridgeInitialized?.Invoke();
        }

        private void Start()
        {
            onBridgeStarted?.Invoke();
        }

        private readonly Dictionary<ScriptableObject, PvMnDeployCell> _charToCellMap = new();
        private readonly Dictionary<ScriptableObject, PvMnSceneUnit> _charToDeployedMeshes = new();

        [SerializeField]
        private PvMnSceneUnit sceneUnitPrefab;

        [SerializeField]
        private PvSoWeaponAndRelicCollection charactersCol;

        [SerializeField]
        private UnityEvent onBridgeInitialized;

        [SerializeField]
        private UnityEvent onBridgeStarted;

        /// <summary>
        /// Binded to Portrait OnInteracted
        /// </summary>
        /// <param name="data"></param>
        /// <param name="targetCell"></param>
        public void HandleSelection(PvSoBattleUnitData data, PvMnDeployCell targetCell)
        {
            if (_charToCellMap.ContainsKey(data) && _charToCellMap[data] == targetCell)
            {
                UndeployUnitMesh(data);
                RemoveCharacterLogically(data);
            }
            else if (_charToCellMap.ContainsKey(data))
            {
                var oldCell = _charToCellMap[data];

                if (targetCell.StandingData != null)
                {
                    UndeployUnitMesh(targetCell.StandingData);
                    RemoveCharacterLogically(targetCell.StandingData);
                }

                oldCell.ClearCell();

                PlaceCharacterLogically(data, targetCell);
                DeployUnitMesh(data, targetCell);
            }
            else
            {
                var oldData = targetCell.StandingData;
                if (oldData)
                {
                    UndeployUnitMesh(targetCell.StandingData);
                    RemoveCharacterLogically(targetCell.StandingData);
                }

                PlaceCharacterLogically(data, targetCell);
                DeployUnitMesh(data, targetCell);
            }
        }

        private void DeployUnitMesh(PvSoBattleUnitData unitData, PvMnDeployCell targetCell)
        {
            if (_charToDeployedMeshes.TryGetValue(unitData, out var deployedSceneUnit))
            {
                deployedSceneUnit.gameObject.SetActive(true);
                deployedSceneUnit.transform.position = targetCell.transform.position;
                return;
            }

            // Instantiate scene unit
            var sceneUnit = Instantiate(sceneUnitPrefab, targetCell.transform.position, Quaternion.identity);

            // Set unit data
            sceneUnit.UnitData = unitData;
            sceneUnit.IsFriendly = true;

            // Initialize unit
            sceneUnit.GenerateNewID();
            _charToDeployedMeshes.Add(unitData, sceneUnit);

            var bodyMeshPrefab = unitData.BodyMeshPrefab;
            if (bodyMeshPrefab)
            {
                var bodyMesh = Instantiate(bodyMeshPrefab, sceneUnit.transform).GetComponent<PvMnMeshPartController>();
                if (!bodyMesh)
                {
                    throw new System.Exception($"ERROR: This Mesh doesn't have {nameof(PvMnMeshPartController)}");
                }

                bodyMesh.transform.localPosition = Vector3.zero;
                bodyMesh.transform.localRotation = Quaternion.identity;
                bodyMesh.transform.localScale = new Vector3(1.75f, 1.75f, 1.75f);

                var headPrefab = unitData.HeadMeshPrefab;
                bodyMesh.InstantiatePartPrefab("head", headPrefab);
            }

            var characterData = PvSaveManager.Instance.GetCharacterEquipmentData(unitData.EntryId);
            var weaponInstanceId = characterData.WeaponInstanceId;
            var weaponInstance = PvSaveManager.Instance.CurrentSaveData.GetWeaponInstance(weaponInstanceId);
            if (weaponInstance != null)
            {
                var weaponData = charactersCol.GetWeaponData(weaponInstance.WeaponDataId);
                if (weaponData)
                {
                    sceneUnit.Initialize(weaponData);
                }
            }
        }

        private void UndeployUnitMesh(ScriptableObject unitData)
        {
            if (_charToDeployedMeshes.TryGetValue(unitData, out var deployedSpawnUnit))
            {
                deployedSpawnUnit.gameObject.SetActive(false);
            }
        }

        private void PlaceCharacterLogically(ScriptableObject data, PvMnDeployCell cell)
        {
            cell.SetCharacter(data);
            _charToCellMap[data] = cell;
        }

        private void RemoveCharacterLogically(ScriptableObject data)
        {
            if (_charToCellMap.TryGetValue(data, out PvMnDeployCell cell))
            {
                cell.ClearCell();
                _charToCellMap.Remove(data);
            }
        }
    }
}
