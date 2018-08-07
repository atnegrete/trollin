using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{

    public float speed;
    public float maxDistance;
    public int damage;
    public GameObject BulletShellPrefab;
    public ParticleSystem PSBulletHitEffect;
    private Vector3 hitPosition;

    private Vector3 firedFromPosition;

    void Start()
    {
        firedFromPosition = this.transform.position;
        hitPosition = Vector3.zero;
        PSBulletHitEffect.Stop();
        GameObject.Instantiate(BulletShellPrefab, this.transform.position + Vector3.back, this.transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(firedFromPosition, transform.position) > maxDistance)
        {
            Destroy(this.gameObject);
        }
    }

    private void FixedUpdate()
    {
        this.transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if we collided with a PlayerPrefab (Collision Layers: Player, Level)
        if(!collision.collider.CompareTag("Player") && !collision.collider.CompareTag("EnemyPlayer"))
        {
            hitPosition = transform.position
                ;
            PSBulletHitEffect.Play();
            Destroy(gameObject, 2f);
        } else
        {
            // If we did, then just destroy with no effects
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
       if(hitPosition != Vector3.zero)
        {
            this.transform.position = hitPosition;
        }
    }


}
