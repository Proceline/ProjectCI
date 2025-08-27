using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Projectiles
{
    /// <summary>
    /// Obtain Projectiles in a Pool
    /// </summary>
    public class PvMnProjectilePool : MonoBehaviour
    {
        private static PvMnProjectilePool _instance;

        private static PvMnProjectilePool GetInstance
        {
            get
            {
                if (!_instance)
                {
                    var poolManager = new GameObject();
                    _instance = poolManager.AddComponent<PvMnProjectilePool>();
                    DontDestroyOnLoad(_instance);
                }

                return _instance;
            }
        }

        private readonly Dictionary<string, Queue<PvMnProjectile>> _projectiles = new();

        private void ReparentProjectileToPool(Transform projectile)
        {
            projectile.SetParent(transform);
        }

        private void AssignParentToProjectile(Transform projectile, Transform targetParent)
        {
            projectile.SetParent(targetParent);
        }

        private Queue<PvMnProjectile> GetProjectilesBackup(string projectileType)
        {
            if (!_projectiles.TryGetValue(projectileType, out var collection))
            {
                collection = new Queue<PvMnProjectile>();
                _projectiles.Add(projectileType, collection);
            }

            return collection;
        }

        private PvMnProjectile TryInitializeProjectile(PvMnProjectile prefab, Transform targetParent)
        {
            var projectileType = prefab.projectileTypeIdentifier;
            var queue = GetProjectilesBackup(projectileType);
            if (queue.TryDequeue(out var result))
            {
                AssignParentToProjectile(result.transform, targetParent);
                result.gameObject.SetActive(true);
                return result;
            }

            if (prefab)
            {
                return Instantiate(prefab, targetParent);
            }

            throw new NullReferenceException("ERROR: Must have a prefab for Creating new Projectile");
        }

        private void DestroyProjectile(PvMnProjectile projectile)
        {
            var projectileType = projectile.projectileTypeIdentifier;
            ReparentProjectileToPool(projectile.transform);
            GetProjectilesBackup(projectileType).Enqueue(projectile);
            projectile.gameObject.SetActive(false);
        }

        public static PvMnProjectile InstantiateProjectile(PvMnProjectile prefab, Transform targetParent = null)
        {
            return GetInstance.TryInitializeProjectile(prefab, targetParent);
        }

        public static void CollectProjectile(PvMnProjectile projectile)
        {
            GetInstance.DestroyProjectile(projectile);
        }
    }
} 