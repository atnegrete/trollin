using GameSparks.RT;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {


    #region RT Packets STuff
    [HideInInspector]
    public RTPlayerInfo RTPlayerInfo { get; set; }
    /// <summary>When the update-packet comes in. This is used to store the new position of the enemy tank</summary>
    [HideInInspector]
    public Vector3 goToPosition;
    /// <summary> When the update packet comes in. This is used to store the new rotation the enemy tank should go to </summary>
    [HideInInspector]
    public Vector3 goToRot;
    public int peerId;
    [HideInInspector]
    public bool isLocalPlayer;
    #endregion

    /// <summary> The rate at which the tank sends out position updates. 0.1f is 6 fps</summary>
    private readonly float mUpdateRate = 0.1f;

    private Camera mainCamera;
    private PlayerGUIPanel playerGUI;
    private Rigidbody myRigidbody;
    private Transform spawnPos;
    private Vector3 moveInput;
    private Vector3 myMoveVelocity;
    private Vector3 myDirection;
    private Vector3 prevPosition;
    private Vector3 velocity;
    private Vector3 diedPosition;

    public Text PlayerName;

    [HideInInspector]
    private int kills;
    public int health;
    [HideInInspector]
    public bool won;
    [HideInInspector]
    public bool dead;
    public float moveSpeed;
    public GameObject deadPrefab;
    [HideInInspector]
    public bool useController;

    public void SetupPlayer(Transform _spawnPos, bool _isPlayer, int peerId)
    {
        this.spawnPos = _spawnPos; // set the spawn position
        this.isLocalPlayer = _isPlayer; // set the player
        this.goToPosition = _spawnPos.transform.position;
        this.peerId = peerId;
        this.health = 100;
        
        if (_isPlayer)
        {
            prevPosition = transform.position;
            myDirection = this.transform.eulerAngles;
        }
    }

    internal void SetUpGSPlayerDetails(GSPlayerDetails gs)
    {
        // Setup player name WorldUI
        PlayerName.text = RTPlayerInfo.displayName;
        transform.Find("PlayerBody").GetComponent<MeshRenderer>().material.color = gs.MaterialColor;
    }

    // Use this for initialization
    void Start () {
        myRigidbody = GetComponent<Rigidbody>();
        GetComponent<ParticleSystem>().Stop();
        playerGUI = FindObjectOfType<PlayerGUIPanel>();

        if (isLocalPlayer)
        {
            AttachListeners();
            mainCamera = FindObjectOfType<Camera>();
            StartCoroutine(SendPlayerMovement());
        }
    }

    private void AttachListeners()
    {
        if (isLocalPlayer)
        {
            GameSparksManager.Instance.GetPacketController().OnPlayerMovementUpdate += OnOtherPlayerMovementUpdate;
            GameSparksManager.Instance.GetPacketController().OnPlayerIsHitEvent += OnOtherPlayerIsHitEvent;
            GameSparksManager.Instance.GetPacketController().OnPlayerIsKilledEvent += OnPlayerIsKilledEvent;
        }
    }

    // Update is called once per frame
    void Update ()
    {
        // Prevent update if player has died
        if(health <= 0 || dead)
        {
            return;
        }

        if (isLocalPlayer)
        {
            LocalMovementLogic();
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, goToPosition, Time.deltaTime / mUpdateRate);
        }
    }

    #region Hit Logic
    private void OnOtherPlayerIsHitEvent(RTPacket packet)
    {
        int playerWhoHitPeerId = packet.Data.GetInt(1) ?? -1;
        if (peerId  == playerWhoHitPeerId) 
        {
            var playerWhoHit = GameSparksManager.Instance.Players.Where(p => p.peerId == playerWhoHitPeerId).First();
        } 
    }

    private void OnPlayerIsKilledEvent(RTPacket packet)
    {
        int playerWhoHitPeerId = packet.Data.GetInt(1) ?? -1;
        int playersAlive = packet.Data.GetInt(2) ?? -1;
        Debug.Log(string.Format("GS | PLAYER KILLED: {0} BY: {1} ", packet.Sender, playerWhoHitPeerId));
        Debug.Log(packet.ToString());

        // Local player killed him >> Update UI
        if (peerId == playerWhoHitPeerId)
        {
            kills++;
            playerGUI.UpdateKillScore(kills);
        }

        var playerWhoKilled = GameSparksManager.Instance.Players.Where(p => p.peerId == playerWhoHitPeerId).First();
        var playerWhoDied = GameSparksManager.Instance.Players.Where(p => p.peerId == packet.Sender).First();
        if(playerWhoDied != null)
        {
            playerWhoDied.OnDeath(playerWhoKilled.RTPlayerInfo, playersAlive);
        }

        #region Check if Won
        // Look through all players & see if I've won, validate against playersAlive from server
        bool allDead = true;
        GameSparksManager.Instance.GetRTSession().ActivePeers.ForEach(a =>
        {
            var player = GameSparksManager.Instance.Players.Where(p => p.peerId == a).First();
            if(player != null && player.peerId != peerId && !player.dead)
            {
                allDead = false;
            }
        });

        if(allDead && playersAlive == 1)
        {
            playerGUI.ShowYouAreDeadOrWon(true);
        } 
        #endregion
    }

    public void NotifyOtherPlayersThatIGotHit(int hitByPlayerId)
    {
        using (RTData data = RTData.Get())
        {
            // Set peerId of player that hit me
            data.SetInt(1, hitByPlayerId); 
            GameSparksManager.Instance.GetRTSession()
                .SendData(RTPacketController.OC_SR_PlayerHitUpdate, GameSparks.RT.GameSparksRT.DeliveryIntent.UNRELIABLE_SEQUENCED, data);
        }
    }

    public void NotifyOtherPlayersThatIGotHitAndDied(int hitByPlayerId)
    {
        using (RTData data = RTData.Get())
        {
            // Set peerId of player that hit me
            data.SetInt(1, hitByPlayerId);
            GameSparksManager.Instance.GetRTSession()
                .SendData(RTPacketController.OC_S_PlayerDeath, GameSparks.RT.GameSparksRT.DeliveryIntent.UNRELIABLE_SEQUENCED, data, RTPacketController.TO_SERVER_ONLY);
        }
    }
    #endregion

    #region Internal Logic
    internal void OnDeath(RTPlayerInfo killedBy, int playersAliveFromServer)
    {
        dead = true;
        diedPosition = myRigidbody.transform.position;
        diedPosition.y = .21f;

        // Deactivate & destroy dead player
        // Spawn dead prefab
        deadPrefab.GetComponent<MeshRenderer>().sharedMaterial.color = RTPlayerInfo.GSPlayerDetails.MaterialColor;
        Instantiate(deadPrefab, diedPosition, deadPrefab.transform.rotation);

        // Move player down
        myRigidbody.transform.position = new Vector3(transform.position.x, -5f, transform.position.z);

        // Show kill log
        playerGUI.OnPlayerKilledAddLog(RTPlayerInfo.displayName, killedBy.displayName);

        // Stop Couroutine for sending movement updates
        StopCoroutine(SendPlayerMovement());

        // Update players alive from server if valid > 0
        if (playersAliveFromServer > 0)
        {
            playerGUI.OnUpdateAlivePlayers(playersAliveFromServer);
        }

        if(isLocalPlayer)
        {
            playerGUI.ShowYouAreDeadOrWon(false);
        }
    }

    internal void OnGotHit(BulletController byBullet, int shooterPeerId)
    {
        // Blood effect
        GetComponent<ParticleSystem>().Emit(20);

        if (isLocalPlayer)
        {
            this.health -= byBullet.damage;

            // Update GUI
            playerGUI.HealthBar.GetComponent<HealthBar>().SetHealth(health);

            // Do OnDeath logic to notify others & update local player
            if (this.health <= 0 && !dead)
            {
                NotifyOtherPlayersThatIGotHitAndDied(shooterPeerId);
            }
            else
            {// took damange
                NotifyOtherPlayersThatIGotHit(shooterPeerId);
            }
        }
    }

    #endregion

    #region Movement Logic

    private void OnOtherPlayerMovementUpdate(RTPacket packet)
    {
        //Debug.Log(packet.ToString());
        var playerList = GameSparksManager.Instance.Players;
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].name == packet.Sender.ToString() && !playerList[i].isLocalPlayer) // Update the sender of the packet
            {
                playerList[i].goToPosition = (Vector3)packet.Data.GetVector3(1); // Update sender's position
                break; // break, because we don’t need to update any other tanks.
            }
        }
    }

    private IEnumerator SendPlayerMovement()
    {
        // We don't want to send position updates until we are actually moving 
        // so we check that the axis-input values are greater or less than zero before sending 
        if ((transform.position != prevPosition) || (Math.Abs(Input.GetAxis("Vertical")) > 0) || (Math.Abs(Input.GetAxis("Horizontal")) > 0))
        {
            // Update previous values
            prevPosition = transform.position;

            using (RTData data = RTData.Get())
            {
                data.SetVector3(1, transform.position);
                GameSparksManager.Instance
                    .GetRTSession()
                    .SendData(RTPacketController.OC_SR_PlayerMovementUpdate, GameSparks.RT.GameSparksRT.DeliveryIntent.UNRELIABLE_SEQUENCED, data);
            }
        }

        yield return new WaitForSeconds(mUpdateRate);
        StartCoroutine(SendPlayerMovement());
    }

    private void LocalMovementLogic()
    {
        if(mainCamera != null)
        {
            // Update Camera
            mainCamera.transform.position = new Vector3(this.transform.position.x, mainCamera.transform.position.y, this.transform.position.z - 2.0f);

            moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            myMoveVelocity = moveInput * moveSpeed;
        }
    }

    private void FixedUpdate()
    {
        if (isLocalPlayer && health > 0)
        {
            myRigidbody.velocity = myMoveVelocity;
        }
    }

    private void LateUpdate()
    {
        if(health <= 0 && dead)
        {
            myRigidbody.transform.position = new Vector3(diedPosition.x, -20f, diedPosition.z);
        }
    }
    #endregion


}
