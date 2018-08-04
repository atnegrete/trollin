using GameSparks.Api.Messages;
using GameSparks.Core;
using GameSparks.RT;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Responses;
using UnityEngine.SceneManagement;
using System.Timers;
using GameSparks.Api.Requests;

public class GameSparksManager : Singleton<GameSparksManager>
{
    // Gamepsarks Required
    public IRTSession RTSession { get; set; }
    private GameSparksRTUnity GameSparksRTUnity;
    private RTSessionInfo SessionInfo;

    // Game Data
    private RTPacketController PacketController;
    private SpawnPoint[] spawnPoints;
    public GameObject PlayerPrefab;
    [HideInInspector]
    public List<PlayerController> Players;
    public GSPlayerDetails GSPlayerDetailsLocal;

    public GameSparksManager()
    {
        PacketController = new RTPacketController();
    }

    #region Gets

    public GameSparksRTUnity GetRTSession()
    {
        return GameSparksRTUnity;
    }

    public RTSessionInfo GetSessionInfo()
    {
        return SessionInfo;
    }

    public RTPacketController GetPacketController()
    {
        return PacketController;
    }

    public int[] GetOpponentPeerIdsOnly()
    {
        return GetRTSession().ActivePeers.Where(a => GetRTSession().PeerId != a).ToArray();
    }

    #endregion

    #region Matchmaking
    public void StartNewRTSession(RTSessionInfo _info)
    {
        GSReset();

        Debug.Log("GSM| Creating New RT Session Instance...");
        SessionInfo = _info;
        if(this.gameObject.GetComponent<GameSparksRTUnity>() == null)
        {
            GameSparksRTUnity = this.gameObject.AddComponent<GameSparksRTUnity>(); // Adds the RT script to the game, if first match after game launched
        }

        // In order to create a new RT game we need a 'FindMatchResponse' //
        // This would usually come from the server directly after a successful MatchmakingRequest //
        // However, in our case, we want the game to be created only when the first player decides using a button //
        // therefore, the details from the response is passed in from the gameInfo and a mock-up of a FindMatchResponse //
        // is passed in. //
        GSRequestData mockedResponse = new GSRequestData()
                                            .AddNumber("port", (double)_info.GetPortID())
                                            .AddString("host", _info.GetHostURL())
                                            .AddString("accessToken", _info.GetAccessToken()); // construct a dataset from the game-details

        FindMatchResponse response = new FindMatchResponse(mockedResponse); // create a match-response from that data and pass it into the game-config
        // So in the game-config method we pass in the response which gives the instance its connection settings //
        // In this example, I use a lambda expression to pass in actions for 
        // OnPlayerConnect, OnPlayerDisconnect, OnReady and OnPacket actions //
        // These methods are self-explanatory, but the important one is the OnPacket Method //
        // this gets called when a packet is received //

        GameSparksRTUnity.Configure(response,
            (peerId) => { OnPlayerConnectedToGame(peerId); },
            (peerId) => { OnPlayerDisconnected(peerId); },
            (ready) => { OnRTReady(ready); },
            (packet) => { PacketController.OnPacketReceived(packet); });
        GameSparksRTUnity.Connect(); // when the config is set, connect the game

        MatchUpdatedMessage.Listener += OnMatchUpdatedMessage;
    }

    public void GSReset()
    {
        // Reset the PacketController
        PacketController = new RTPacketController();

        // Reset the Player Controllers
        Players.ToList().ForEach(p => Destroy(p.gameObject));
        Players = new List<PlayerController>();

        // Remove Game Scene loading event handler
        SceneManager.sceneLoaded -= OnGameSceneLoaded;  
    }

    private void OnPlayerConnectedToGame(int _peerId)
    {
        Debug.Log("GSM| Player Connected, " + _peerId);
        // Handle logic of a player connecting to game late
        var data = new RTData();
        data.SetFloat(1, GSPlayerDetailsLocal.color.red);
        data.SetFloat(2, GSPlayerDetailsLocal.color.green);
        data.SetFloat(3, GSPlayerDetailsLocal.color.blue);
        GetRTSession().SendData(RTPacketController.OC_SR_OnPlayerLateJoinGetOtherPlayerDetails, GameSparksRT.DeliveryIntent.RELIABLE, data);
    }

    private void OnMatchUpdatedMessage(MatchUpdatedMessage m)
    {
        var participantsJoined = m.Participants.Where(p => m.AddedPlayers.Contains(p.Id)).ToList();
        participantsJoined.ForEach(pj =>
        {
            // Find spawn point
            SpawnPoint spawn = spawnPoints.Where(s => s.playerPeerId == pj.PeerId).First();
            if(spawn != null)
            {
                // Create player info
                RTPlayerInfo playerInfo = new RTPlayerInfo(pj.DisplayName, pj.Id, (int) pj.PeerId);
                InstantiateConnectedPlayer(spawn, playerInfo);
            }
        });
    }

    private void OnPlayerDisconnected(int _peerId)
    {
        Debug.Log("GSM| Player Disconnected, " + _peerId);
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].name == _peerId.ToString())
            {
                Players[i].gameObject.SetActive(false);
                break;
            }
        }
    }

    private void OnRTReady(bool _isReady)
    {
        if (_isReady)
        {
            Debug.Log("GSM| RT Session Connected...");
            SceneManager.LoadScene(LoadingManager.GAME_SCENE);
            SceneManager.sceneLoaded += OnGameSceneLoaded;
        }
    }

    private void OnGameSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        spawnPoints = FindObjectsOfType(typeof(SpawnPoint)) as SpawnPoint[];

        #region Setup Player 
        Players = new List<PlayerController>();
        List<GameObject> allPlayersPrefabs = new List<GameObject>();
        Players.ToList().ForEach(p => allPlayersPrefabs.Add(PlayerPrefab));

        Debug.Log("GC| Found " + GetSessionInfo().GetRTPlayerList().Count + " Players...");

        for (int playerIndex = 0; playerIndex < GameSparksManager.Instance.GetSessionInfo().GetRTPlayerList().Count; playerIndex++)
        {
            for (int spawnerIndex = 0; spawnerIndex < spawnPoints.Length; spawnerIndex++)
            {
                InstantiateConnectedPlayer(spawnPoints[spawnerIndex], GameSparksManager.Instance.GetSessionInfo().GetRTPlayerList()[playerIndex]);
            }
        }
        #endregion

        PacketController.SendPlayerReady(GSPlayerDetailsLocal);
    }

    private void InstantiateConnectedPlayer(SpawnPoint spawnPoint, RTPlayerInfo playerInfo)
    {

        if (spawnPoint.playerPeerId == playerInfo.peerId)
        {
            GameObject newPlayer = Instantiate(PlayerPrefab, spawnPoint.gameObject.transform.position, spawnPoint.gameObject.transform.rotation) as GameObject;
            newPlayer.name = playerInfo.peerId.ToString();
            newPlayer.transform.SetParent(this.transform);
            bool isLocalPlayer = playerInfo.peerId == GetRTSession().PeerId;
            newPlayer.GetComponent<PlayerController>().SetupPlayer(spawnPoint.gameObject.transform, isLocalPlayer, playerInfo.peerId);

            PlayerController addingPlayer = newPlayer.GetComponent<PlayerController>();
            addingPlayer.RTPlayerInfo = playerInfo;
            addingPlayer.SetUpGSPlayerDetails(addingPlayer.RTPlayerInfo.GSPlayerDetails);
            Players.Add(addingPlayer);
        }
    }

    #endregion
}
