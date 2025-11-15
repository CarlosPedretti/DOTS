using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public class UniqueObject : MonoBehaviour
    {
        [SerializeField]
        private bool persistAcrossScenes = false;

        private static HashSet<string> uniqueObjects;
        [SerializeField] private bool isInitialObject;

        private void Awake()
        {
            // Inicializa el conjunto si es nulo
            if (uniqueObjects == null)
            {
                uniqueObjects = new HashSet<string>();
            }

            string objectId = $"{GetType()}_{gameObject.name}";

            if (uniqueObjects.Contains(objectId))
            {
                //Debug.LogWarning($"UniqueObjects already contains this object: {objectId}. Destroying it!");
                isInitialObject = false;
                Destroy(gameObject);
                return;
            }

            uniqueObjects.Add(objectId);
            isInitialObject = true;

            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnDestroy()
        {
            string objectId = $"{GetType()}_{gameObject.name}";

            if (isInitialObject)
            {
                uniqueObjects.Remove(objectId);
            }

            //Debug.LogWarning($"{objectId} destroyed.");
        }

        [Command]
        private void TestUniqueObjectsList()
        {
            Debug.Log($"Objects in List: {uniqueObjects.Count}");

            foreach (string objectId in uniqueObjects)
            {
                Debug.Log($"ObjectID: {objectId}");
            }
        }
    }
}
