using UnityEngine;

namespace Utilities
{
    public abstract class ObjectC : MonoBehaviour
    {
        [SerializeField] private string id;

        public string Id => id;
    }
}
