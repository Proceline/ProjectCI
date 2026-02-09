using Assets.IndAssets.Scripts.Deployment;
using IndAssets.Scripts.Managers;
using ProjectCI.CoreSystem.Runtime.Saving;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
            onDeployCellFocus.action.canceled += OnDeployCellConfirmed;
        }

        private void OnDestroy()
        {
            onDeployCellFocus.action.canceled -= OnDeployCellConfirmed;
        }

        private readonly Dictionary<ScriptableObject, PvDeployCell> _charToCellMap = new();
        private readonly Dictionary<ScriptableObject, PvMnSceneUnit> _charToDeployedMeshes = new();

        [SerializeField]
        private PvMnSceneUnit sceneUnitPrefab;

        [SerializeField]
        private PvSoWeaponAndRelicCollection charactersCol;

        [SerializeField]
        private UnityEvent onBridgeInitialized;

        [SerializeField]
        private UnityEvent onBridgeStarted;

        [SerializeField]
        private InputActionReference onDeployCellFocus;

        [SerializeField]
        private Camera deployInteractCamera;

        [SerializeField]
        private UnityEvent<RaycastHit[]> onInteractedObjectsApplied;

        private void OnDeployCellConfirmed(InputAction.CallbackContext context)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);
            onInteractedObjectsApplied?.Invoke(hits);
        }

        /// <summary>
        /// Binded to Portrait OnInteracted
        /// </summary>
        /// <param name="data"></param>
        /// <param name="targetCell"></param>
        public void HandleSelection(PvSoBattleUnitData data, PvDeployCell targetCell)
        {
            if (_charToCellMap.TryGetValue(data, out var oldCell))
            {
                UndeployUnitMesh(data);
                RemoveCharacterLogically(data);
            }

            var existedData = targetCell.StandingData;
            if (existedData)
            {
                UndeployUnitMesh(existedData);
                RemoveCharacterLogically(existedData);

                if (oldCell)
                {
                    PlaceCharacterLogically(existedData, oldCell);
                    DeployUnitMesh(existedData as PvSoBattleUnitData, oldCell);
                }
            }

            if (targetCell != oldCell)
            {
                PlaceCharacterLogically(data, targetCell);
                DeployUnitMesh(data, targetCell);
            }
        }

        private void DeployUnitMesh(PvSoBattleUnitData unitData, PvDeployCell targetCell)
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

        private void PlaceCharacterLogically(ScriptableObject data, PvDeployCell cell)
        {
            cell.SetCharacter(data);
            _charToCellMap[data] = cell;
        }

        private void RemoveCharacterLogically(ScriptableObject data)
        {
            if (_charToCellMap.TryGetValue(data, out PvDeployCell cell))
            {
                cell.ClearCell();
                _charToCellMap.Remove(data);
            }
        }

        /// <summary>
        /// Binded and Used in Portrait
        /// </summary>
        /// <param name="dataObj"></param>
        /// <param name="targetImage"></param>
        public void UpdateCharacterIcon(ScriptableObject dataObj, Image targetImage)
        {
            if (_charToCellMap.ContainsKey(dataObj))
            {
                targetImage.color = Color.green;
            }
            else
            {
                targetImage.color = Color.white;
            }
        }
    }
}
