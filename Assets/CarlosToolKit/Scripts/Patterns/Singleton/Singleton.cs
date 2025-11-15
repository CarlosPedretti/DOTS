using UnityEngine;

namespace Utilities
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        [SerializeField] bool dontDestroyOnLoad = false;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    //instance = FindFirstObjectByType<T>();

                    //if (instance == null)
                    //{
                    //    GameObject singletonObject = new GameObject(typeof(T).Name);
                    //    instance = singletonObject.AddComponent<T>();
                    //}
                }
                return instance;
            }
        }

        public static bool Exists => instance != null;
        public bool IsReady { get; private set; }
        protected void SetReady() => IsReady = true;


        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;

                if (dontDestroyOnLoad)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void Start()
        {
            if (!IsReady)
                SetReady();
        }
    }
}
