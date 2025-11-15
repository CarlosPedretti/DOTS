
namespace Utilities
{
    public class BasicRecyclableObject : RecyclableObject
    {
        private void OnDisable()
        {

            Recycle();
        }

        public override void OnSpawned() { }

        public override void OnRecycled() { }

    }
}