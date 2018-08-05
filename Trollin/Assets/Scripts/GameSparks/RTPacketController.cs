using UnityEngine;
using UnityEditor;
using GameSparks.RT;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Timers;

public class RTPacketController 
{
    public static readonly int[] TO_SERVER_ONLY = new int[] { 0 };

    public delegate void RTPacketTriggerEvent();
    public delegate void RTPacketEventInt(int value);
    public delegate void RTPacketEventDoubleInt(int val1, int val2);
    public delegate void RTPacketPlayerMovementUpdate(RTPacket packet);
    public delegate void RTPacketEvent(RTPacket packet);

    public event RTPacketEventInt OnMatchUpdateCountdownTimer;
    public event RTPacketEventInt OnMatchUpdatePlayersActive;
    public event RTPacketEvent OnPlayerIsHitEvent;
    public event RTPacketEvent OnPlayerIsKilledEvent;

    public event RTPacketTriggerEvent OnMatchStartTrigger;
    public event RTPacketPlayerMovementUpdate OnPlayerRotationUpdate;
    public event RTPacketPlayerMovementUpdate OnPlayerMovementUpdate;
    public event RTPacketPlayerMovementUpdate OnPlayerFiringUpdate;

    #region OP Codes
    public const int OC_SR_PlayerReady = 1;
    public const int OC_SR_PlayerRotationUpdate = 4;
    public const int OC_SR_PlayerMovementUpdate = 2;
    public const int OC_SR_PlayerColorUpdate = 3;
    public const int OC_SR_PlayerFiringUpdate = 51;
    public const int OC_SR_PlayerHitUpdate = 52;
    public const int OC_S_PlayerDeath = 300;
    public const int OC_R_PlayerDeath = 301;
    public const int OC_R_MatchCountdownTimer = 100;
    public const int OC_R_MatchStart = 101;
    public const int OC_SR_OnPlayerLateJoinGetOtherPlayerDetails = 500; // Only sends to other players
    #endregion

    public void OnPacketReceived(RTPacket packet)
    {
        /*
         * Op Codes:
         * 1 to 50 : Reserved for Info, Movement etc
         * 51 to 99 : Reserved for Fighting (Hit, death, etc)
         * 100 to * : Reserved for Cloud Logic
         */
        switch(packet.OpCode)
        {
            case OC_SR_PlayerReady: // Received player ready packet (from another player).
                ReceivedPlayerReady(packet);
                break;
            case OC_SR_PlayerMovementUpdate: // Player Info Update Received 
                ReceivedPlayerMovementUpdate(packet);
                break;
            case OC_SR_PlayerRotationUpdate: // Player Info Update Received 
                ReceivedPlayerRotationUpdate(packet);
                break;
            case OC_SR_PlayerColorUpdate: // Received player ready packet (from another player).
                ReceivedOtherPlayerColorUpdate(packet);
                break;
            case OC_SR_PlayerFiringUpdate: // Player has shot a bullet
                ReceivedPlayerFiringUpdate(packet);
                break;
            case OC_SR_PlayerHitUpdate: // Player has gotten hit by a bullet
                ReceivedPlayerIsHitUpdate(packet);
                break;
            case OC_R_PlayerDeath: // Player has died
                ReceivedPlayerIsKilledUpdate(packet);
                break;
            case OC_R_MatchCountdownTimer: // Update match countdown timer. (from cloud countdown)
                ReceivedMatchUpdateCountdownTimer(packet);
                break;
            case OC_R_MatchStart: // Countdown finished, start the match.
                ReceivedMatchStartTrigger(packet);
                break;
            case OC_SR_OnPlayerLateJoinGetOtherPlayerDetails: // Countdown finished, start the match.
                ReceivedOtherPlayerDetails(packet);
                break;
        }
    }

    #region Fighting
    private void ReceivedPlayerIsHitUpdate(RTPacket packet)
    {
        if (OnPlayerIsHitEvent != null)
        {
            OnPlayerIsHitEvent(packet);
        }
    }

    private void ReceivedPlayerIsKilledUpdate(RTPacket packet)
    {
        if (OnPlayerIsKilledEvent != null)
        {
            OnPlayerIsKilledEvent(packet);
        }
    }

    private void ReceivedPlayerFiringUpdate(RTPacket packet)
    {
        if (OnPlayerFiringUpdate != null)
        {
            OnPlayerFiringUpdate(packet);
        }
    }
    #endregion

    #region Updates & Movement

    public void SendPlayerReady(GSPlayerDetails playerDetails)
    {
        var manager = GameSparksManager.Instance;
        manager.GetRTSession().SendData(OC_SR_PlayerReady, GameSparksRT.DeliveryIntent.UNRELIABLE_SEQUENCED, null, TO_SERVER_ONLY);
        if (OnMatchUpdatePlayersActive != null)
        {
            OnMatchUpdatePlayersActive(manager.GetRTSession().ActivePeers.Count);
        }

        var data = new RTData();
        data.SetFloat(1, playerDetails.color.red);
        data.SetFloat(2, playerDetails.color.green);
        data.SetFloat(3, playerDetails.color.blue);
        int[] opponents = manager.GetRTSession().ActivePeers.ToList().Where(p => p != playerDetails.peerId).ToArray();
        manager.GetRTSession().SendData(OC_SR_PlayerReady, GameSparksRT.DeliveryIntent.UNRELIABLE_SEQUENCED, data, opponents);
    }

    private void ReceivedPlayerReady(RTPacket packet)
    {
        Debug.Log(packet.ToString());
        var senderPlayer = GameSparksManager.Instance.Players.ToList().Where(p => p.peerId == packet.Sender).First();
        if (senderPlayer != null)
        {
            senderPlayer.RTPlayerInfo.GSPlayerDetails.color.red = (float) packet.Data.GetFloat(1);
            senderPlayer.RTPlayerInfo.GSPlayerDetails.color.green = (float) packet.Data.GetFloat(2);
            senderPlayer.RTPlayerInfo.GSPlayerDetails.color.blue = (float) packet.Data.GetFloat(3);
            senderPlayer.transform.Find("PlayerBody").GetComponent<MeshRenderer>().material.color = senderPlayer.RTPlayerInfo.GSPlayerDetails.MaterialColor;
        }
    }

    private void ReceivedOtherPlayerColorUpdate(RTPacket packet)
    {
        Debug.Log(packet.ToString());
        var senderPlayer = GameSparksManager.Instance.Players.ToList().Where(p => p.peerId == packet.Sender).First();
        if (senderPlayer != null)
        {
            senderPlayer.RTPlayerInfo.GSPlayerDetails.color.red = (float)packet.Data.GetFloat(1);
            senderPlayer.RTPlayerInfo.GSPlayerDetails.color.green = (float)packet.Data.GetFloat(2);
            senderPlayer.RTPlayerInfo.GSPlayerDetails.color.blue = (float)packet.Data.GetFloat(3);
            senderPlayer.transform.Find("PlayerBody").GetComponent<MeshRenderer>().material.color = senderPlayer.RTPlayerInfo.GSPlayerDetails.MaterialColor;
        }
    }

    private void ReceivedOtherPlayerDetails(RTPacket packet)
    {
        Debug.Log(packet.ToString());
        var player = GameSparksManager.Instance.Players.Where(p => p.peerId == packet.Sender).First();
        if(player != null && player.RTPlayerInfo != null)
        {
            // Update the GS Player Details
            player.RTPlayerInfo.GSPlayerDetails.color.red = (float)packet.Data.GetFloat(1);
            player.RTPlayerInfo.GSPlayerDetails.color.green = (float)packet.Data.GetFloat(2);
            player.RTPlayerInfo.GSPlayerDetails.color.blue = (float)packet.Data.GetFloat(3);

            // Then actually update the Game Model for rendering
            if(player != null)
            {
                player.transform.Find("PlayerBody").GetComponent<MeshRenderer>().material.color = player.RTPlayerInfo.GSPlayerDetails.MaterialColor;
            }
        }
        
    }

    private void ReceivedPlayerMovementUpdate(RTPacket packet)
    {
        if(OnPlayerMovementUpdate != null)
        {
            OnPlayerMovementUpdate(packet);
        }
    }

    private void ReceivedPlayerRotationUpdate(RTPacket packet)
    {
        if (OnPlayerRotationUpdate != null)
        {
            OnPlayerRotationUpdate(packet);
        }
    }

    #endregion

    #region Receiving Cloud Updates
    public void ReceivedMatchUpdateCountdownTimer(RTPacket packet)
    {
        var timeLeft = packet.Data.GetInt(1) ?? 0;
        var active = packet.Data.GetInt(2);
        if(OnMatchUpdateCountdownTimer != null) {
            OnMatchUpdateCountdownTimer(timeLeft);
        }
        if(OnMatchUpdatePlayersActive != null && active != null)
        {
            OnMatchUpdatePlayersActive(GameSparksManager.Instance.GetRTSession().ActivePeers.Count);
        }
    }

    public void ReceivedMatchStartTrigger(RTPacket packet)
    {
        if(OnMatchStartTrigger != null)
        {
            OnMatchStartTrigger();
        }
    }
    #endregion

    internal void NotifyGlobalTimedEvents(object sender, ElapsedEventArgs e)
    {
        // Update Active Players
        if (OnMatchUpdatePlayersActive != null) OnMatchUpdatePlayersActive(GameSparksManager.Instance.GetRTSession().ActivePeers.Count);
    }

}