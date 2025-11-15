using System;
using UnityEngine;
using Utilities;

namespace Utilities
{
    public class MyObject : RecyclableObject
    {
        public override void OnSpawned()
        {
            Invoke(nameof(Recycle), 5);
        }

        public override void OnRecycled()
        {
            Debug.Log("Reciclado");
        }
    }
}

