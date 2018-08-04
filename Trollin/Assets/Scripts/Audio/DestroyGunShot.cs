using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyGunShot : MonoBehaviour {

    private float totalTimeBeforeDestroy;

	// Use this for initialization
	void Start () {
        var sound = this.GetComponent<AudioSource>();
        totalTimeBeforeDestroy = sound.clip.length;
	}
	
	// Update is called once per frame
	void Update () {
        totalTimeBeforeDestroy -= Time.deltaTime;

        if(totalTimeBeforeDestroy <= 0f)
        {
            Destroy(this.gameObject);
        }
	}
}
