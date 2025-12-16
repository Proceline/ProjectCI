using UnityEngine;
using System.Collections.Generic;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    /// <summary>
    /// Controller for managing mesh part roots and their transformations
    /// Allows easy access to part roots (like headroot) and instantiating prefabs at specific locations
    /// </summary>
    public class PvMnMeshPartController : MonoBehaviour
    {
        [System.Serializable]
        public class PartRootData
        {
            [SerializeField] private string partName;
            [SerializeField] private Transform rootTransform;
            [SerializeField] private Transform sampleTransform;

            public string PartName => partName;
            public Transform RootTransform => rootTransform;
            public Vector3 LocalPosition => sampleTransform.localPosition;
            public Quaternion LocalRotation => sampleTransform.localRotation;
            public Vector3 LocalScale => sampleTransform.localScale;

            public PartRootData(string name, Transform root, Transform sample)
            {
                partName = name;
                rootTransform = root;
                sampleTransform = sample;
            }
        }

        [Header("Mesh Part Roots")]
        [SerializeField] private List<PartRootData> partRoots = new List<PartRootData>();

        /// <summary>
        /// Get part root transform by name
        /// </summary>
        public Transform GetPartRoot(string partName)
        {
            if (string.IsNullOrEmpty(partName))
                return null;

            foreach (var partRoot in partRoots)
            {
                if (partRoot.PartName == partName && partRoot.RootTransform != null)
                {
                    return partRoot.RootTransform;
                }
            }

            // Try to find in hierarchy if not found in list
            return FindChild(transform, partName);
        }

        /// <summary>
        /// Instantiate a prefab at the specified part root
        /// </summary>
        public GameObject InstantiatePartPrefab(string partName, GameObject prefab)
        {
            var root = GetPartRoot(partName);
            if (root == null)
            {
                Debug.LogWarning($"Part root '{partName}' not found on {gameObject.name}");
                return null;
            }

            if (prefab == null)
            {
                Debug.LogWarning($"Prefab is null for part '{partName}'");
                return null;
            }

            var instance = Instantiate(prefab, root);
            
            var partData = partRoots.Find(p => p.PartName == partName);
            if (partData != null)
            {
                instance.transform.localPosition = partData.LocalPosition;
                instance.transform.localRotation = partData.LocalRotation;
                instance.transform.localScale = partData.LocalScale;
            }

            return instance;
        }

        /// <summary>
        /// Clear all children from a part root
        /// </summary>
        public void ClearPartRoot(string partName)
        {
            var root = GetPartRoot(partName);
            if (root == null)
            {
                Debug.LogWarning($"Part root '{partName}' not found on {gameObject.name}");
                return;
            }

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Destroy(root.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Find child transform by name recursively
        /// </summary>
        private Transform FindChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                    return child;

                var result = FindChild(child, childName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
