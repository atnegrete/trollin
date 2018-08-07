using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{

    public float speed;
    public float maxDistance;
    public int damage;
    public AudioSource GunShotSound;
    public GameObject BulletShellPrefab;
    public ParticleSystem PSBulletHitEffect;
    private Vector3 hitPosition;

    private Vector3 firedFromPosition;

    void Start()
    {
        firedFromPosition = this.transform.position;
        GunShotSound.Play();
        PSBulletHitEffect.Stop();
        GameObject.Instantiate(BulletShellPrefab, this.transform.position + Vector3.back, this.transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (Vector3.Distance(firedFromPosition, transform.position) > maxDistance)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Level"))
        {
            PSBulletHitEffect.Play();
            hitPosition = this.transform.position;
            Destroy(gameObject, 1.5f);
        }
    }

    private void LateUpdate()
    {
        if(hitPosition != null)
        {
            this.transform.position = hitPosition;
        }
    }
}
