using GameSparks.RT;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour {

    public bool isFiring;
    [HideInInspector]
    public BulletController bullet;
    [HideInInspector]
    public PlayerController player;

    public float timeBetweenShots;
    public Transform firePoint;

    private float shotCounter;

    // Use this for initialization
    void Start () {
        this.player = GetComponentInParent<PlayerController>();
        if(player.isLocalPlayer)
        {
            GameSparksManager.Instance.GetPacketController().OnPlayerShotBulletUpdate += OnOpponentShotBullet;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (player.isLocalPlayer)
        {
            if (isFiring)
            {
                shotCounter -= Time.deltaTime;
                if (shotCounter <= 0)
                {
                    shotCounter = timeBetweenShots;
                    ShootBullet();
                }
            }
            else
            {
                shotCounter = 0;
            }
        }
	}

    private void ShootBullet()
    {
        using (RTData data = RTData.Get())
        {
            // Dispatch Packet to notify other players to create this bullet
            Guid guid = Guid.NewGuid();
            data.SetString(1, guid.ToString());
            GameSparksManager.Instance.GetRTSession().SendData(RTPacketController.OC_SR_PlayerFiredUpdate, GameSparks.RT.GameSparksRT.DeliveryIntent.UNRELIABLE, data);


            // Create bullet for this local player
            InstantiateBullet((int)GameSparksManager.Instance.GetRTSession().PeerId, guid.ToString());
        }
    }

    private void InstantiateBullet(int _ownerPeerId, string _uid)
    {
        BulletController newBullet = Instantiate(bullet, firePoint.position, firePoint.rotation) as BulletController;
        newBullet.CreateBullet(_ownerPeerId, _uid);
    }

    private void OnOpponentShotBullet(RTPacket packet)
    {
        var players = GameSparksManager.Instance.Players;
        for (int i = 0; i < players.Count; i++)
        {
            // Find player that shot the bullet
            if (players[i].name == packet.Sender.ToString())
            {
                var gun = players[i].GetComponentInChildren<GunController>();
                gun.InstantiateBullet((int)packet.Sender, packet.Data.GetString(1));
            }
        }
    }
}
