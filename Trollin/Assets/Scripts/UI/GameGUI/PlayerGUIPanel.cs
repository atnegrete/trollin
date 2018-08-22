using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using GameSparks.RT;

public class PlayerGUIPanel : MonoBehaviour {

    public delegate void UpdateAmmoUI(RTPacket packet);
    public event UpdateAmmoUI OnUpdateAmmoUI;

    public Text AlivePlayerCountText;
    public Text MatchCountdownText;
    public Text LocalPlayerKills;
    public Text KillsLog;
    public Text YouAreDead;
    public Text AmmoText;
    public GameObject HealthBar;

    private List<string> mKillLogs;
    private readonly float mLogUpdateSecsRate = 20;
    private GunController[] localPlayerGunControllers;

    // Use this for initialization
    void Start () {
        // Bind to Countdown
        GameSparksManager.Instance.GetPacketController().OnMatchUpdateCountdownTimer += OnUpdateMatchCountdown;
        GameSparksManager.Instance.GetPacketController().OnMatchUpdatePlayersActive += OnUpdateAlivePlayers;
        GameSparksManager.Instance.GetPacketController().OnMatchStartTrigger += OnMatchStart;
        mKillLogs = new List<string>();

        LocalPlayerKills.text = "Kills: 0";
        YouAreDead.enabled = false;
        StartCoroutine(UpdateLog());

        // Get local player
        AttemptGetLocalPlayer();
    }

    private void Update()
    {
        // Set the current gun controller's ammo
        if(localPlayerGunControllers != null)
        {
            var gun = localPlayerGunControllers.Where(g => g.isActiveAndEnabled).First();
            if(gun != null)
            {
                AmmoText.text = String.Format("{0} | {1}", gun.currentAmmoInMag, gun.ExtraAmmo);
            }
        } else
        {
            AttemptGetLocalPlayer();
        }
    }

    private void AttemptGetLocalPlayer()
    {
        // Get local player
        var player = GameSparksManager.Instance.Players.Where(p => p.isLocalPlayer).First();
        if (player != null)
        {
            localPlayerGunControllers = player.GetComponentsInChildren<GunController>();
        }
    }

    public void UpdateKillScore(int kills)
    {
        LocalPlayerKills.text = "Kills: " + kills;
    }

    private void OnMatchStart()
    {
        MatchCountdownText.text = String.Empty;

        // Delete all the walls
        List<GameObject> walls = GameObject.FindGameObjectsWithTag("MatchCountdownWalls").ToList();
        if(walls != null)
        {
            walls.ForEach(w => Destroy(w));
        }
    }

    public void OnUpdateAlivePlayers(int alivePlayers)
    {
        if(AlivePlayerCountText)
            AlivePlayerCountText.text = "Alive Players: " + alivePlayers;
    }

    void OnUpdateMatchCountdown(int countdown)
    {
        if(MatchCountdownText)
            MatchCountdownText.text = "Match starts in ... " + countdown;
    }

    public void ShowYouAreDeadOrWon(bool winner)
    {
        if(winner)
            YouAreDead.text = "Wohoo, you're a weenner!";

        YouAreDead.enabled = true;
    }

    public void OnPlayerKilledAddLog(string killed, string killedBy)
    {
        string log = String.Format("{0} was killed by {1}!", killed, killedBy);
        mKillLogs.Add(log);
        StopCoroutine(UpdateLog());
        StartCoroutine(UpdateLog());
    }

    private IEnumerator UpdateLog()
    {
        // Clear current logs
        KillsLog.text = String.Empty;

        // Clear logs from top down
        if (mKillLogs.Count > 0)
        {
            string logResult = String.Empty;
            // Update text
            mKillLogs.ForEach(log =>
            {
                logResult += String.Format("{0} \n", log);
            });
            KillsLog.text = logResult;

            // Remove first entry for next time it reloads
            mKillLogs.RemoveAt(0);
        }

        yield return new WaitForSeconds(mLogUpdateSecsRate);
        StartCoroutine(UpdateLog());
    }
}
