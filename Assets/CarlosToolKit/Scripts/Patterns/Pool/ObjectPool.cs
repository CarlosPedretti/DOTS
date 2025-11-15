using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

namespace Utilities
{
    /// <summary>
    /// Manages the creation, reuse, and recycling of RecyclableObjects.
    /// </summary>
    public class ObjectPool<T> : IDisposable, IObjectPoolGeneric where T : MonoBehaviour, IPoolable
    {
        private readonly T prefab;
        private readonly HashSet<T> instantiatedObjects;
        private readonly Transform parentTransform;
        private Queue<T> recycledObjects;
        private readonly int maxPoolSize;

        public ObjectPool(T prefab, int maxPoolSize = 0, Transform parentTransform = null)
        {
            this.prefab = prefab;
            this.parentTransform = parentTransform;
            this.maxPoolSize = maxPoolSize;
            instantiatedObjects = new HashSet<T>();
        }

        /// <summary>
        /// Initializes the pool with a specified number of inactive objects.
        /// </summary>
        public void Initialize(int numberOfInitialObjects)
        {
            if (recycledObjects != null)
            {
                Debug.LogWarning($"Pool for '{prefab.name}' already initialized.");
                return;
            }

            recycledObjects = new Queue<T>(numberOfInitialObjects);

            for (int i = 0; i < numberOfInitialObjects; i++)
            {
                var instance = InstantiateNewInstance();
                instance.transform.SetParent(parentTransform);
                instance.gameObject.SetActive(false);
                instance.SetCanBeRecycled(true);
                recycledObjects.Enqueue(instance);
            }
        }

        /// <summary>
        /// Spawns (activates and returns) an instance of type <typeparamref name="T"/> from the pool.
        /// The instance will be positioned and rotated as specified, and optionally parented under the given transform.
        /// If the instance is null or its GameObject has been destroyed, a warning is logged and null is returned.
        /// </summary>
        /// <param name="position">The world position to place the spawned object (optional).</param>
        /// <param name="rotation">The rotation to apply to the spawned object (optional).</param>
        /// <param name="parent">The transform to parent the spawned object under (optional).</param>
        /// <returns>
        /// An active and initialized instance of type <typeparamref name="T"/> from the pool, or null if the instance is invalid.
        /// </returns>
        public T Spawn(Vector3 position = default, Quaternion rotation = default, Transform parent = default)
        {
            var obj = GetInstance();

            if (obj == null || obj.gameObject == null)
            {
                Debug.LogWarning("Tried to recycle a null or destroyed object.");
                return null;
            }

            obj.transform.SetParent(parent);
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.gameObject.SetActive(true);
            obj.OnSpawned();

            return obj;
        }

        /// <summary>
        /// Recycles an object back into the pool after use.
        /// </summary>
        public void RecycleGameObject(T obj)
        {
            if (obj == null)
            {
                Debug.LogWarning("Tried to recycle a destroyed or null object.");
                return;
            }

            try
            {
                obj.transform.SetParent(parentTransform);

                obj.gameObject.SetActive(false);
                obj.OnRecycled();
                recycledObjects.Enqueue(obj);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Recycles an object back into the pool after use.
        /// </summary>
        public void RecycleGameObject(IPoolable obj)
        {
            RecycleGameObject(obj as T);
        }

        private T GetInstance()
        {
            while (recycledObjects.Count > 0)
            {
                var obj = recycledObjects.Dequeue();
                if (obj != null && obj.gameObject != null)
                    return obj;
            }

            if (!HasReachedMaxPoolSize()) Debug.LogWarning($"Not enough recycled objects for '{prefab.name}'. Consider increasing the initial pool size.");

            var instance = InstantiateNewInstance();
            instance?.SetCanBeRecycled(true);
            return instance;
        }

        private T InstantiateNewInstance()
        {
            if (HasReachedMaxPoolSize())
            {
                Debug.LogWarning($"Pool size limit reached for '{prefab.name}'. Max Pool Size was set at {maxPoolSize}");

                return null;
            }

            var instance = UnityEngine.Object.Instantiate(prefab);
            instance.SetPool(this);
            instantiatedObjects.Add(instance);

            return instance;
        }

        private bool HasReachedMaxPoolSize()
        {
            instantiatedObjects.RemoveWhere(obj => obj == null || obj.gameObject == null);

            return instantiatedObjects.Count >= maxPoolSize && maxPoolSize != 0;
        }

        public void Dispose()
        {
            foreach (var obj in instantiatedObjects)
            {
                if (obj != null) UnityEngine.Object.Destroy(obj.gameObject);
            }

            instantiatedObjects.Clear();
            recycledObjects?.Clear();
        }
    }
}