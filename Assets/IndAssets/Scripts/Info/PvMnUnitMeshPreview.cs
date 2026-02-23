using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;

namespace IndAssets.Scripts.Info
{
    public class PvMnUnitMeshPreview : MonoBehaviour
    {
        [SerializeField] private PvSoBattleUnitData unitData;
        [SerializeField] private bool spawnOnStart = true;

        public PvSoBattleUnitData UnitData => unitData;

        private void Start()
        {
            if (spawnOnStart)
            {
                InitializeMesh();
            }
        }

        public void InitializeMesh()
        {
            if (!unitData)
            {
                return;
            }

            var root = transform.childCount > 0 ? transform.GetChild(0) : transform;

            var bodyMeshPrefab = unitData.BodyMeshPrefab;
            if (bodyMeshPrefab)
            {
                var bodyMesh = Instantiate(bodyMeshPrefab, root).GetComponent<PvMnMeshPartController>();
                bodyMesh.transform.localPosition = Vector3.zero;
                bodyMesh.transform.localRotation = Quaternion.identity;

                var headPrefab = unitData.HeadMeshPrefab;
                bodyMesh.InstantiatePartPrefab("head", headPrefab);
            }
        }
    }
}