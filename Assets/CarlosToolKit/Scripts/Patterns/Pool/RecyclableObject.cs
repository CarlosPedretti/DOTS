using UnityEngine;

namespace Utilities
{
    /// <summary>
    /// Base abstract class for objects that can be recycled by an ObjectPool.
    /// </summary>
    public abstract class RecyclableObject : ObjectC, IPoolable
    {
        public object Pool { get { return pool; } }
        private object pool;

        public bool CanBeRecycled { get { return canBeRecycled; } }
        private bool canBeRecycled;

        /// <summary>
        /// Sets the recyclable object with its pool reference.
        /// </summary>
        public void SetPool(object obj)
        {
            pool = obj;
        }

        /// <summary>
        /// Sets if the object can be recycled.
        /// </summary>
        public void SetCanBeRecycled(bool value)
        {
            canBeRecycled = value;
        }

        /// <summary>
        /// Attempts to recycle the object back into the pool.
        /// </summary>
        public void Recycle()
        {
            if (!canBeRecycled || Pool == null)
            {
                return;
            }

            if (Pool is IObjectPoolGeneric poolGeneric)
            {
                poolGeneric.RecycleGameObject(this);
            }
            else
            {
                Debug.LogError("Pool is not a valid IObjectPoolGeneric. Cannot recycle object.");
            }
        }



        /// <summary>
        /// Called when the object is spawned from the pool.
        /// </summary>
        public abstract void OnSpawned();

        /// <summary>
        /// Called when the object is recycled and returned to the pool.
        /// </summary>
        public abstract void OnRecycled();
    }
}
