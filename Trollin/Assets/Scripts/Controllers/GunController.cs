using GameSparks.RT;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour {

    [HideInInspector]
    public bool isFiringHeld;
    public bool shouldFire;
    private float shotDelayCounter;

    public PlayerAimAndFireController fireController;
    public BulletController bullet;
    public PlayerController player;
    public float timeBetweenShots;

    // Use this for initialization
    void Start () {
        this.player = GetComponentInParent<PlayerController>();
        if(player.isLocalPlayer)
        {
            GameSparksManager.Instance.GetPacketController().OnPlayerFiringUpdate += OnFiringUpdate;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (isFiringHeld)
        {
            shotDelayCounter -= Time.deltaTime;
            if (shotDelayCounter <= 0)
            {
                shotDelayCounter = timeBetweenShots;
                fireController.lazer.enabled = true;
                shouldFire = true;

                DoFireEffects();
            } else
            {
                fireController.lazer.enabled = false;
                shouldFire = false;
            }
        }
        else
        {
            shotDelayCounter = 0;
        }
    }

    private void DoFireEffects()
    {
    }

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
                int[] otherPlayers = GameSparksManager.Instance.GetRTSession().ActivePeers.Where(a => a != player.peerId).ToArray();
                GameSparksManager.Instance.GetRTSession()
                    .SendData(RTPacketController.OC_SR_PlayerFiringUpdate, GameSparks.RT.GameSparksRT.DeliveryIntent.UNRELIABLE, data, otherPlayers);

                if (isFiringHeld)
                {
                    StartFiring(player.peerId);
                }
            }
        }
    }

    private void StartFiring(int ownerPeerId)
    {
        //BulletController newBullet = Instantiate(bullet, firePoint.position, firePoint.rotation) as BulletController;
        //newBullet.CreateBullet(_ownerPeerId, _uid);
    }

    private void OnFiringUpdate(RTPacket packet)
    {
        var players = GameSparksManager.Instance.Players;
        for (int i = 0; i < players.Count; i++)
        {
            // Find player that shot the bullet
            if (players[i].name == packet.Sender.ToString())
            {
                var gun = players[i].GetComponentInChildren<GunController>();
                gun.isFiringHeld = packet.Data.GetInt(1) > 0;
            }
        }
    }

}
