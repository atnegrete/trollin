using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour {

    Rigidbody shellRigidbody;

	// Use this for initialization
	void Start () {
        shellRigidbody = GetComponent<Rigidbody>();
        shellRigidbody.AddExplosionForce(Random.Range(20f, 50f), transform.right * -1, 1f);
        Destroy(this.gameObject, 5);
    }

    private void Update()
    {

    }
}
