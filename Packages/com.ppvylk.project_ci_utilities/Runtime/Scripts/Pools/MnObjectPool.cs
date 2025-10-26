using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.Utilities.Runtime.Pools
{
    /// <summary>
    /// A generic GameObject pool that manages objects by prefab type.
    /// Automatically creates a static instance and cleans up on scene change.
    /// </summary>
    public class MnObjectPool : MonoBehaviour
    {
        private static MnObjectPool _instance;
        
        // Dictionary to store pools by prefab instance ID
        private readonly Dictionary<int, Queue<GameObject>> _pools = new();
        
        // Dictionary to store prefab references for instantiation
        private readonly Dictionary<int, GameObject> _prefabReferences = new();

        /// <summary>
        /// Get the singleton instance of the object pool
        /// </summary>
        public static MnObjectPool Instance
        {
            get
            {
                if (!_instance)
                {
                    // Create a new GameObject for the pool manager
                    GameObject poolObject = new GameObject("MnObjectPool");
                    _instance = poolObject.AddComponent<MnObjectPool>();
                }

                return _instance;
            }
        }

        /// <summary>
        /// Get a GameObject from the pool for the specified prefab
        /// </summary>
        /// <param name="prefab">The prefab to get an instance of</param>
        /// <returns>A GameObject instance from the pool or newly created</returns>
        public GameObject Get(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("MnObjectPool: Cannot get object from null prefab!");
                return null;
            }
            
            int prefabId = prefab.GetInstanceID();
            Debug.Log($"MnObjectPool: Getting object from prefab {prefab.name} with ID {prefabId}");
            
            // Initialize pool for this prefab if it doesn't exist
            if (!_pools.ContainsKey(prefabId))
            {
                _pools[prefabId] = new Queue<GameObject>();
                _prefabReferences[prefabId] = prefab;
            }
            
            GameObject obj;
            
            // Try to get from pool first
            if (_pools[prefabId].Count > 0)
            {
                obj = _pools[prefabId].Dequeue();
                obj.SetActive(true);
            }
            else
            {
                // Create new instance if pool is empty
                obj = Instantiate(prefab);
            }
            
            return obj;
        }
        
        /// <summary>
        /// Return a GameObject to the pool
        /// </summary>
        /// <param name="obj">The GameObject to return to the pool</param>
        public void Return(GameObject obj)
        {
            if (!obj) return;
            
            // Find which prefab this object belongs to
            int prefabId = FindPrefabId(obj);
            
            if (prefabId == -1)
            {
                Destroy(obj);
                throw new Exception($"MnObjectPool: Cannot return object {obj.name} - no matching prefab found!");
            }
            
            // Deactivate the object and add to pool
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            _pools[prefabId].Enqueue(obj);
        }
        
        /// <summary>
        /// Clear all objects in the pool
        /// </summary>
        public void Clear()
        {
            foreach (var pool in _pools.Values)
            {
                while (pool.Count > 0)
                {
                    GameObject obj = pool.Dequeue();
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
            }
            
            _pools.Clear();
            _prefabReferences.Clear();
        }
        
        /// <summary>
        /// Get the number of objects in the pool for a specific prefab
        /// </summary>
        /// <param name="prefab">The prefab to check</param>
        /// <returns>Number of objects in pool for this prefab</returns>
        public int GetPoolCount(GameObject prefab)
        {
            if (prefab == null) return 0;
            
            int prefabId = prefab.GetInstanceID();
            return _pools.ContainsKey(prefabId) ? _pools[prefabId].Count : 0;
        }
        
        private int FindPrefabId(GameObject obj)
        {
            // Try to find the prefab ID by checking if this object was instantiated from any of our tracked prefabs
            foreach (var kvp in _prefabReferences)
            {
                GameObject prefab = kvp.Value;
                if (prefab != null && obj.name.StartsWith(prefab.name))
                {
                    return kvp.Key;
                }
            }
            
            return -1;
        }
        
        private void Awake()
        {
            // Ensure only one instance exists
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void OnDestroy()
        {
            // Clear all pooled objects when the pool is destroyed
            Clear();
            
            // Reset static instance reference
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
