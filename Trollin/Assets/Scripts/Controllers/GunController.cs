using GameSparks.RT;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunController : MonoBehaviour {

    [HideInInspector]
    public bool isFiringHeld;
    [HideInInspector]
    public bool shouldFire;
    public string GunFireAudioName;
    [Range(.01f, 1f)]
    public float timeBetweenShots;
    [Range(1, 10)]
    public int ShotsPerFireEffects;
    [Range(1, 200)]
    public int MagazineSize;
    [Range(0f, 3f)]
    public float reloadTime = 1.5f;
    [Range(0, 1000)]
    public float ExtraAmmo = 100;
    public PlayerAimAndFireController fireController;
    public BulletController bullet;
    public PlayerController playerController;
    public Animator GunAnimator;
    public GameObject magazinePrefab;

    private float shotDelayCounter;
    private int shotsFiredForEffects;
    [HideInInspector]
    public int currentAmmoInMag;
    private bool isReloading = false;
    private bool dropMag = false;
  
    // Use this for initialization
    void Start () {
        currentAmmoInMag = MagazineSize;
        this.playerController = GetComponentInParent<PlayerController>();
        if(playerController.isLocalPlayer)
        {
            GameSparksManager.Instance.GetPacketController().OnPlayerFiringUpdate += OnFiringUpdate;
            GameSparksManager.Instance.GetPacketController().OnPlayerReload += OnPlayerReload;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (isReloading)
        {
            return;
        }

        if(currentAmmoInMag <= 0 && playerController.isLocalPlayer)
        {
            if(ExtraAmmo > 0)
            {
                StartCoroutine(Reload());
                NotifyReloading();
            }
            return;
        }

        if (isFiringHeld)
        {
            shotDelayCounter -= Time.deltaTime;
            if (shotDelayCounter <= 0)
            {
                shotDelayCounter = timeBetweenShots;
                shouldFire = true;
                shotsFiredForEffects--;
                currentAmmoInMag--;
                if (shotsFiredForEffects <= 0)
                {
                    DoFireEffects();
                    shotsFiredForEffects = ShotsPerFireEffects;
                }
            }
            else
            {
                shouldFire = false;
                ShotsPerFireEffects = 0;
            }
        }
        else
        {
            shotDelayCounter = 0;
            ShotsPerFireEffects = 0;
            shouldFire = false;
        }
    }

    private void OnEnable()
    {
        isReloading = false;
        GunAnimator.SetBool("Reloading", false);
    }

    private void DoFireEffects()
    {
        AudioManager.Instance.Play(GunFireAudioName, transform.position);
    }

    #region RT Packets

    public void NotifyFiring(bool toFire)
    {
        if(isFiringHeld != toFire)
        {
            // Set gun to firing
            isFiringHeld = toFire;

            using (RTData data = RTData.Get())
            {
                // Dispatch Packet to notify other players to start firing
                data.SetInt(1, toFire ? 1 : -1);
                int[] otherPlayers = GameSparksManager.Instance.GetRTSession().ActivePeers.Where(a => a != playerController.peerId).ToArray();
                GameSparksManager.Instance.GetRTSession()
                    .SendData(RTPacketController.OC_SR_PlayerFiringUpdate, GameSparks.RT.GameSparksRT.DeliveryIntent.UNRELIABLE, data, otherPlayers);
            }
        }
    }

    private void OnFiringUpdate(RTPacket packet)
    {
        var player = GameSparksManager.Instance.Players.Where(p => p.peerId == packet.Sender).FirstOrDefault();
        if(player != null)
        {
            var gun = player.GetComponentInChildren<GunController>();
            gun.isFiringHeld = packet.Data.GetInt(1) > 0;
        }
    }

    public void NotifyReloading()
    {
        using (RTData data = RTData.Get())
        {
            int[] otherPlayers = GameSparksManager.Instance.GetRTSession().ActivePeers.Where(a => a != playerController.peerId).ToArray();
            GameSparksManager.Instance.GetRTSession()
                .SendData(RTPacketController.OC_SR_PlayerReload, GameSparks.RT.GameSparksRT.DeliveryIntent.UNRELIABLE, data, otherPlayers);
        }
    }

    private void OnPlayerReload(RTPacket packet)
    {
        var player = GameSparksManager.Instance.Players.Where(p => p.peerId == packet.Sender).FirstOrDefault();
        if (player != null )
        {
            var gun = player.GetComponentInChildren<GunController>();
            if(gun != null)
            {
                StartCoroutine(gun.Reload());
            }
        }
    }

    #endregion

    public IEnumerator Reload()
    {
        Debug.Log("Reloading");
        isReloading = true;
        shouldFire = false;
        GunAnimator.SetBool("Reloading", true);

        AudioManager.Instance.Play("ReloadRifle", transform.position);
        yield return new WaitForSeconds(reloadTime - .25f);
        GunAnimator.SetBool("Reloading", false);
        yield return new WaitForSeconds(.25f);

        dropMag = true;
        currentAmmoInMag = MagazineSize;
        ExtraAmmo -= MagazineSize;
        isReloading = false;
    }

    private void LateUpdate()
    {
        if(dropMag)
        {
            DropMag();
            dropMag = false;
        }
    }

    void DropMag()
    {
        var mag = Instantiate(magazinePrefab, transform.position, transform.rotation);
        mag.AddComponent<Rigidbody>();
        mag.layer = LayerMask.NameToLayer("Ammo");
        mag.AddComponent<CapsuleCollider>();
        mag.transform.localScale += new Vector3(4f, 4f, 4f);
        mag.GetComponent<Rigidbody>().useGravity = true;
        mag.GetComponent<Rigidbody>().mass = 5f;
        mag.GetComponent<Rigidbody>().AddForce((mag.transform.position - playerController.transform.position) * 100f);
        Destroy(mag, 10);
    }
}
