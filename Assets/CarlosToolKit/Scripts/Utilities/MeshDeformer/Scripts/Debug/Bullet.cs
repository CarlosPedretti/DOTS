using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public float killTime = 3;

    private void Update()
    {
        killTime -= Time.deltaTime;

        if (killTime <= 0)
            Destroy(gameObject);
    }

    // Use this for initialization
    public void Shoot (float _force)
    {
        GetComponent<Rigidbody>().AddForce(transform.forward * _force, ForceMode.VelocityChange);
	}
}