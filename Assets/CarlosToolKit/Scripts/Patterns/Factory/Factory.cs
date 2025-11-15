using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public class Factory : MonoBehaviour
    {

        [SerializeField] private ObjectC[] objects;

        private Dictionary<string, ObjectC> idToObject;


        private void Awake()
        {
            idToObject = new Dictionary<string, ObjectC>();

            foreach (var obj in objects)
            {
                idToObject.Add(obj.Id, obj);
            }
        }

        public ObjectC Create(string id, Transform objectPosition)
        {

            if (!idToObject.TryGetValue(id, out var _object))
            {
                Debug.Log("Object with id: " + id + " does not exist");
            }

            return Instantiate(_object, objectPosition.transform);
        }
    }
}
