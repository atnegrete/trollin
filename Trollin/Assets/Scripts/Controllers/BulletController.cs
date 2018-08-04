using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour {

    public float speed;
    public int damage;
    public int bouncyness = 1;

    /// <summary> The peerId of the player that fired this bullet </summary>
    [HideInInspector]
    public int ownerPeerId;

    [SerializeField]
    private GameObject GunShotSound;

    /// <summary>This just keeps track of the time with Time.deltaTime</summary>
    private float countDownTimer;
    /// <summary>The duration the shell will stay enabled</summary>
    private readonly float mLifeTime = 1f;
    private bool hasCollided = false;

    public void CreateBullet(int ownerPeerId, string uid)
    {
        this.gameObject.name = uid;
        this.ownerPeerId = ownerPeerId;
        this.countDownTimer = 0f;
        this.gameObject.SetActive(true);

        GameObject gunShot = Instantiate(GunShotSound, this.transform.position, this.transform.rotation) as GameObject;
    }

    // Update is called once per frame
    void Update () {
        this.transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // counts down as the update continues //
        if (gameObject.activeSelf)
        {
            countDownTimer += Time.deltaTime;
            if (countDownTimer >= mLifeTime)
            { // if the timer reaches the lifetime, then disable the game object
                Destroy(this.gameObject);
                countDownTimer = 0f; // ... and reset the timer
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "BulletIgnore")
        {
            return;
        }

        // Prevent multiple collisions before deactivation.
        if (hasCollided) return;
        hasCollided = true;

        if (other.gameObject.tag == "Player")
        {
            var playerHit = other.GetComponent<PlayerController>();
            if (playerHit != null)
            {
                if(playerHit.peerId != ownerPeerId)
                {
                    playerHit.OnGotHit(this);
                    // Destroy this bullet
                    Destroy(this.gameObject);
                } 
            }
        }

        // Destroy this bullet
        Destroy(this.gameObject);
    }
}
