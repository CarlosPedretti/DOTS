using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{

    /// <summary>
    /// Singleton that manages multiple ObjectPools identified by keys.
    /// </summary>
    public class PoolsManager : Singleton<PoolsManager>
    {
        [SerializeField] private List<PoolsEntry> prefabEntriesList = new List<PoolsEntry>();
        [SerializeField] private bool usePoolAsParent;

        private Dictionary<string, PoolsEntry> poolsDictionary = new Dictionary<string, PoolsEntry>();

        private new void Awake()
        {
            base.Awake();
            Initialize();
        }

        private void Initialize()
        {
            if (prefabEntriesList == null || prefabEntriesList.Count == 0) return;

            foreach (var entry in prefabEntriesList)
            {
                if (!poolsDictionary.ContainsKey(entry.Key.ToLowerInvariant()))
                {
                    CreatePool(entry);
                    poolsDictionary.Add(entry.Key.ToLowerInvariant(), entry);
                }
            }
        }

        private void CreatePool(PoolsEntry entry)
        {
            if (!(entry.Prefab is IPoolable))
            {
                Debug.LogError($"Prefab '{entry.Prefab.name}' does not implement IPoolable.");
                return;
            }

            Type prefabType = entry.Prefab.GetType();

            var method = typeof(PoolsManager).GetMethod(nameof(CreateGenericPool), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var genericMethod = method.MakeGenericMethod(prefabType);
            genericMethod.Invoke(this, new object[] { entry });
        }

        private void CreateGenericPool<T>(PoolsEntry entry) where T : MonoBehaviour, IPoolable
        {
            var prefab = entry.Prefab as T;
            var pool = new ObjectPool<T>(prefab, entry.MaxPoolSize, usePoolAsParent ? transform : null);
            pool.Initialize(entry.PoolSize);
            entry.Pool = pool;
        }

        /// <summary>
        /// Spawns (retrieves) an object of type <typeparamref name="T"/> from the object pool associated with the specified key.
        /// The object will be positioned and rotated as specified, and optionally parented under the given transform.
        /// If no pool is found with the provided key, a warning will be logged and null will be returned.
        /// </summary>
        /// <typeparam name="T">The type of MonoBehaviour that implements IPoolable, expected by the pool.</typeparam>
        /// <param name="key">The unique key identifying the desired object pool.</param>
        /// <param name="position">The position where the spawned object should be placed (optional).</param>
        /// <param name="rotation">The rotation to apply to the spawned object (optional).</param>
        /// <param name="parent">The transform to parent the spawned object under (optional).</param>
        /// <returns>
        /// An instance of type <typeparamref name="T"/> from the pool, or null if the pool does not exist or the cast fails.
        /// </returns>
        public T Spawn<T>(string key, Vector3 position = default, Quaternion rotation = default, Transform parent = default) where T : MonoBehaviour, IPoolable
        {
            if (poolsDictionary.TryGetValue(key.ToLowerInvariant(), out PoolsEntry entry))
            {
                var pool = entry.Pool as ObjectPool<T>;
                return pool?.Spawn(position, rotation, parent);
            }

            Debug.LogWarning($"No pool found with key '{key}'.");
            return null;
        }

        /// <summary>
        /// Recycles an object back into its associated pool by key.
        /// </summary>
        public void Recycle<T>(string key, T obj) where T : MonoBehaviour, IPoolable
        {
            if (poolsDictionary.TryGetValue(key.ToLowerInvariant(), out PoolsEntry entry))
            {
                var pool = entry.Pool as ObjectPool<T>;
                pool?.RecycleGameObject(obj);
            }
            else
            {
                Debug.LogWarning($"No pool found with key '{key}'. Destroying object.");
                GameObject.Destroy(obj.gameObject);
            }
        }

        /// <summary>
        /// Retrieves the pool associated with the given key.
        /// </summary>
        public ObjectPool<T> GetPool<T>(string key) where T : MonoBehaviour, IPoolable
        {
            if (poolsDictionary.TryGetValue(key.ToLowerInvariant(), out PoolsEntry entry))
            {
                return entry.Pool as ObjectPool<T>;
            }

            Debug.LogWarning($"No pool found with key '{key}'.");
            return null;
        }
    }



    [System.Serializable]
    public class PoolsEntry
    {
        [Tooltip("Unique key to identify this pool. Used to spawn or recycle objects.")]
        public string Key;

        [Tooltip("Initial number of objects to instantiate and keep inactive in the pool.")]
        public int PoolSize;

        [Tooltip("Maximum number of objects this pool can instantiate. Set to 0 for unlimited.")]
        public int MaxPoolSize;

        [Tooltip("The prefab that will be pooled and reused.")]
        public MonoBehaviour Prefab; // Debe implementar IPoolable

        [HideInInspector]
        public object Pool; // Referencia al ObjectPool<T>, pero como object por ser genérico
    }
}



