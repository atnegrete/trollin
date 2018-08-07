using GameSparks.RT;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSystem : MonoBehaviour {

    public int selectedWeapon = 0;
    public int previousSelectedWeapon = 0;
    public PlayerAimAndFireController PlayerAimAndFireController;
    public PlayerController PlayerController;

	// Use this for initialization
	void Start () {
		if(PlayerController.isLocalPlayer)
        {
            GameSparksManager.Instance.GetPacketController().OnPlayerChangedWeapon += OnPlayerChangedWeapon;
        }

        SelectWeapon();
    }
	
	// Update is called once per frame
	void Update () {

        if(PlayerController.isLocalPlayer)
        {
            LocalWeaponSystemSelection();
        }

        if (previousSelectedWeapon != selectedWeapon)
        {
            if(PlayerController.isLocalPlayer)
            {
                NotifyOthersAboutSelectedWeapon();
            }

            SelectWeapon();
        }
    }

    private void LocalWeaponSystemSelection()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (selectedWeapon >= transform.childCount - 1)
            {
                selectedWeapon = 0;
            }
            else
            {
                selectedWeapon++;
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (selectedWeapon <= transform.childCount - 1)
            {
                selectedWeapon = transform.childCount - 1;
            }
            else
            {
                selectedWeapon--;
            }
        }

        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedWeapon = 0;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && transform.childCount >= 2)
        {
            selectedWeapon = 1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && transform.childCount >= 3)
        {
            selectedWeapon = 2;
        }

        if (Input.GetKeyDown(KeyCode.Alpha4) && transform.childCount >= 4)
        {
            selectedWeapon = 3;
        }
    }

    private void SelectWeapon()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var weapon = transform.GetChild(i);
            if (i == selectedWeapon)
            {
                weapon.gameObject.SetActive(true);

                // Update the FireAndAimController to the selected gun
                PlayerAimAndFireController.gun = weapon.GetComponent<GunController>();
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }
        }
        previousSelectedWeapon = selectedWeapon;
    }

    public void ManuallySetSelectedWeapon(int sel)
    {
        selectedWeapon = sel;
    }

    #region RT Packets
    private void OnPlayerChangedWeapon(RTPacket packet)
    {
        var playerControllerWhoChangedWeapon = GameSparksManager.Instance.Players.Where(p => p.peerId == packet.Sender).First();
        var selectedWeapon = packet.Data.GetInt(1) ?? -1;
        if(playerControllerWhoChangedWeapon != null && selectedWeapon >= 0)
        {
            playerControllerWhoChangedWeapon.GetComponentInChildren<WeaponSystem>().ManuallySetSelectedWeapon(selectedWeapon);
        }
    }

    internal void NotifyOthersAboutSelectedWeapon()
    {
        using (RTData data = RTData.Get())
        {
            // Set peerId of player that hit me
            data.SetInt(1, selectedWeapon);
            GameSparksManager.Instance.GetRTSession()
                .SendData(RTPacketController.OC_SR_PlayerChangedWeapon, GameSparks.RT.GameSparksRT.DeliveryIntent.UNRELIABLE_SEQUENCED, data);
        }
    }
    #endregion


}
