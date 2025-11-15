namespace Utilities
{
    public interface IPoolable
    {
        object Pool { get; }
        bool CanBeRecycled { get; }

        void Recycle();
        void SetPool(object pool);
        void SetCanBeRecycled(bool value);
        void OnSpawned();
        void OnRecycled();
    }

    public interface IObjectPoolGeneric
    {
        void RecycleGameObject(IPoolable obj);
    }
}
