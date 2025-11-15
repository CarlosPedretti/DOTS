using QFSW.QC;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities
{
    public class Consumer : MonoBehaviour
    {
        [SerializeField] private BasicRecyclableObject prefab;
        private ObjectPool<RecyclableObject> objectPool;
        [SerializeField] bool testNormalPool;
        [SerializeField] Transform spawnPoint;

        private void Awake()
        {
            if (!testNormalPool) return;
            objectPool = new ObjectPool<RecyclableObject>(prefab, parentTransform: this.transform);
            objectPool.Initialize(3);
        }

        [Command]
        private void SpawnNormal()
        {
            var myObject = objectPool.Spawn(parent: spawnPoint);
        }

        [Command]
        private void SpawnManager(string key)
        {
            PoolsManager.Instance.Spawn<BasicRecyclableObject>(key, parent: spawnPoint);
        }
    }
}

