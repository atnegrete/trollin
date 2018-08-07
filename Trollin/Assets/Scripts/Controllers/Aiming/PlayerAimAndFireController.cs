using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using GameSparks.RT;

public class PlayerAimAndFireController : MonoBehaviour
{
    public bool RotationSmooth = true;
    public Transform WeaponPivot;
    public Transform PlayerModel;
    public GameObject GunFirePosition;

    public string NoAimOffsetTag = "NoAimOffset";
    public string LevelTag = "Level";
    public string EnemyTag = "Enemy";
    Transform playerTransform;

    public LineRenderer lazer;
    public GunController gun;
    PlayerController playerController;
    Rigidbody playerBody;

    private readonly float mUpdateRate = 0.1f;
    private Vector3 prevWeaponPivotRotation;
    private Vector3 goToTargetAimPoint;
    private Vector3 goToAimRealPosition;
    private Vector3 targetAimPoint;

    void Start ()
    {
        Init();
	}

    void Init()
    {
        playerBody = GetComponentInChildren<Rigidbody>();
        playerTransform = transform;
        playerController = GetComponentInParent<PlayerController>();

        prevWeaponPivotRotation = playerBody.transform.rotation.eulerAngles;

        if (playerController.isLocalPlayer)
        {
            GameSparksManager.Instance.GetPacketController().OnPlayerRotationUpdate += OnOtherPlayerRotationUpdate;
            StartCoroutine(SendPlayerRotation());
        }
    }

    private void Update()
    {

        if (playerController.isLocalPlayer)
        {
            UsingMouse();
        }

        if (gun != null && gun.shouldFire)
        {
            RaycastHit hit;
            BulletController newBullet = Instantiate(gun.bullet, GunFirePosition.transform.position, GunFirePosition.transform.rotation) as BulletController;

            Debug.DrawRay(GunFirePosition.transform.position, (goToTargetAimPoint - GunFirePosition.transform.position));

            // Only shoot ray if it's not the local player, since we always deal damage only from opponents point of view.
            if (Physics.Raycast(GunFirePosition.transform.position, (goToTargetAimPoint - GunFirePosition.transform.position), out hit, gun.bullet.maxDistance) && !gun.playerController.isLocalPlayer)
            {
                var playerBodyControllerHit = hit.collider.gameObject.GetComponentInParent<PlayerController>();
                // Dispatch on HitEvent if it wasn't self-inflicted damage (shouldn't happen)
                if (playerBodyControllerHit != null && gun.playerController.peerId != playerBodyControllerHit.peerId) 
                {
                    playerBodyControllerHit.OnGotHit(gun.bullet, gun.playerController.peerId);
                }
            }
        }

        if (gun != null)
        {
            gun.fireController.lazer.enabled = gun.shouldFire;
        }
    }

    void FixedUpdate ()
    {
        if(playerController.isLocalPlayer)
        {
            RotatePlayer();
            RotateWeaponPivot();
        }
        else
        {
            if(goToAimRealPosition != null)
            {
                // Update Other Client Player rotation
                //Quaternion pRotation = Quaternion.FromToRotation(playerTransform.forward, (goToAimRealPosition - playerTransform.position).normalized) * playerBody.rotation;
                var targetRotation = Quaternion.LookRotation(goToAimRealPosition - playerBody.transform.position);
                playerBody.transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / mUpdateRate);

                // Update Other Client RotateWeaponPivot
                Quaternion wRotation = Quaternion.FromToRotation(WeaponPivot.forward, (goToTargetAimPoint - WeaponPivot.position).normalized) * WeaponPivot.rotation;
                WeaponPivot.rotation = Quaternion.Slerp(WeaponPivot.rotation, Quaternion.Euler((Vector2)wRotation.eulerAngles), Time.deltaTime/mUpdateRate);
            }
        }
    }

    #region Local Aiming Controlling
    void RotatePlayer()
    {
        Quaternion rotation = Quaternion.FromToRotation(playerTransform.forward, (AimController.GetAimRealPosition() - playerTransform.position).normalized) * playerBody.rotation;
        if (RotationSmooth)
        {
            playerBody.MoveRotation(Quaternion.Slerp(playerBody.rotation, Quaternion.Euler(rotation.eulerAngles.y * Vector3.up), Time.fixedDeltaTime * 20f));
        } else
        {
            playerBody.MoveRotation(Quaternion.Euler(rotation.eulerAngles.y * Vector3.up));
        }
    }

    void RotateWeaponPivot()
    {
        float aimY = AimController.GetAimRealPosition().y;
        float playerY = playerTransform.position.y;
        float targetY = AimController.GetTargetPosition().y;

        float yDist = Mathf.Abs(playerY - targetY);
        targetAimPoint = AimController.GetAimRealPosition();

        if(AimController.TargetTag == NoAimOffsetTag)
        {
            targetAimPoint.y = playerY + 1f;
        }

        if (AimController.TargetTag == LevelTag)
        {
            yDist = Mathf.Abs(playerY - aimY);

            if (yDist > 0.3f)
            {
                targetAimPoint.y = aimY + 1f;
            }
            else
            {
                targetAimPoint.y = playerY + 1f;
            }
        }

        if (AimController.TargetTag == EnemyTag)
        {
            if (yDist > 0.3f)
            {
                targetAimPoint.y = aimY;
            }
            else
            {
                targetAimPoint.y = targetY + 1f;
            }
        }

        Quaternion rotation = Quaternion.FromToRotation(WeaponPivot.forward, (targetAimPoint - WeaponPivot.position).normalized) * WeaponPivot.rotation;
        WeaponPivot.rotation = Quaternion.Euler((Vector2)rotation.eulerAngles);
    }

    #endregion

    #region Firing Logic
    private void UsingMouse()
    {
        gun.NotifyFiring(Input.GetMouseButton(0));
    }
    #endregion

    private IEnumerator SendPlayerRotation()
    {
        // We don't want to send position updates until we are actually moving 
        // so we check that the axis-input values are greater or less than zero before sending 
        if (WeaponPivot.transform.eulerAngles != prevWeaponPivotRotation)
        {
            // Update previous values
            prevWeaponPivotRotation = WeaponPivot.transform.eulerAngles;

            using (RTData data = RTData.Get())
            {
                data.SetVector3(1, targetAimPoint);
                data.SetVector3(2, AimController.GetAimRealPosition());
                GameSparksManager.Instance
                    .GetRTSession()
                    .SendData(RTPacketController.OC_SR_PlayerRotationUpdate, GameSparks.RT.GameSparksRT.DeliveryIntent.UNRELIABLE_SEQUENCED, data);
            }
        }

        yield return new WaitForSeconds(mUpdateRate);
        StartCoroutine(SendPlayerRotation());
    }

    private void OnOtherPlayerRotationUpdate(RTPacket packet)
    {
        //Debug.Log(packet.ToString());
        var playerControllerSender = GameSparksManager.Instance.Players.Where(p => p.peerId == packet.Sender).First();
        if(playerControllerSender != null && !playerControllerSender.isLocalPlayer)
        {
            var aimAndFireController = playerControllerSender.GetComponent<PlayerAimAndFireController>();
            aimAndFireController.goToTargetAimPoint = (Vector3)packet.Data.GetVector3(1);
            aimAndFireController.goToAimRealPosition = (Vector3)packet.Data.GetVector3(2);
        }
    }
}
