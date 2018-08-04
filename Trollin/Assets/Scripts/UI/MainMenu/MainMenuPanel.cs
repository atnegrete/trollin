using GameSparks.Api.Messages;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using GameSparks.RT;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using System.Text;

public class MainMenuPanel : MonoBehaviour {

    public Button FindMatchButton;
    public Text MatchStatusText;
    public ColorPicker ColorPicker;
    public GameObject MainMenuPlayerPrefab;

    private RTSessionInfo tempRTSessionInfo;
    private bool mPlayerHasChangedColor;

    void Awake () {
        mPlayerHasChangedColor = false;
        FindMatchButton.onClick.AddListener(FindMatch);
        MatchNotFoundMessage.Listener += OnMatchNotFound;

        // Get player Details
        var gSPlayerDetailsLocal = GameSparksManager.Instance.GSPlayerDetailsLocal;

        // Set color
        MainMenuPlayerPrefab.GetComponent<MeshRenderer>().material.color = gSPlayerDetailsLocal.MaterialColor;
        ColorPicker.Result = gSPlayerDetailsLocal.MaterialColor;
    }

    private void FindMatch()
    {
        BlockInput();
        MatchStatusText.text = "Searching for matches...";
        MatchmakingRequest request = new MatchmakingRequest();
        request.SetMatchShortCode("SC_MatchTest");
        request.SetSkill(1);
        request.Send(OnMatchmakingSuccess, OnMatchmakingError);

        // Update color preference if it has changed
        if(mPlayerHasChangedColor)
        {
            var color = ColorPicker.PUBLIC_GetColor();
            long red = (long)(color.r * 100L);
            long green = (long)(color.g * 100L);
            long blue = (long)(color.b * 100L);

            LogEventRequest_UP_Player_Details playerDetailsRequest = new LogEventRequest_UP_Player_Details();
            Debug.Log(string.Format("GSM EVENT| RED {0}, G {1}, B {2}", red, green, blue));
            playerDetailsRequest
                .Set_RED(red)
                .Set_GREEN(green)
                .Set_BLUE(blue)
                .Send((response) =>
                {
                    // TODO: Check for errors
                });
        }

    }

    private void OnMatchmakingSuccess(MatchmakingResponse obj)
    {
        MatchFoundMessage.Listener += OnMatchFoundUpdateUI;
    }

    private void OnMatchFoundUpdateUI(MatchFoundMessage _message)
    {
        MatchStatusText.text = "Match found! Setting up game...";
        //GameSparksManager.Instance.OnMatchFound(_message);

        Debug.Log("Match Found!...");
        StringBuilder sBuilder = new StringBuilder();
        sBuilder.AppendLine("Match Found...");
        sBuilder.AppendLine("Host URL:" + _message.Host);
        sBuilder.AppendLine("Port:" + _message.Port);
        sBuilder.AppendLine("Access Token:" + _message.AccessToken);
        sBuilder.AppendLine("MatchId:" + _message.MatchId);
        sBuilder.AppendLine("Opponents:" + _message.Participants.Count());
        sBuilder.AppendLine("_________________");
        sBuilder.AppendLine(); // we'll leave a space between the player-list and the match data

        // Set the session info
        tempRTSessionInfo = new RTSessionInfo(_message);

        foreach (GameSparks.Api.Messages.MatchFoundMessage._Participant player in _message.Participants)
        {
            sBuilder.AppendLine("Player:" + player.PeerId + " User Name:" + player.DisplayName); // add the player number and the display name to the list

            // Create a GS Player Details for each player
            GSPlayerDetails matchPlayerDetails = new GSPlayerDetails() {
                peerId = (int)player.PeerId,
                playerId = player.Id
            };

            // For myself, keep my details (it has color) but update with peerId
            if(matchPlayerDetails.playerId == GameSparksManager.Instance.GSPlayerDetailsLocal.playerId)
            {
                tempRTSessionInfo.GetRTPlayerList().Where(pi => pi.id == player.Id).First().GSPlayerDetails = GameSparksManager.Instance.GSPlayerDetailsLocal;
                tempRTSessionInfo.GetRTPlayerList().Where(pi => pi.id == player.Id).First().GSPlayerDetails.peerId = matchPlayerDetails.peerId;
            } else
            {
                tempRTSessionInfo.GetRTPlayerList().Where(pi => pi.id == player.Id).First().GSPlayerDetails = matchPlayerDetails;
            }
        }

        StartGSGameSession();
    }

    private void StartGSGameSession()
    {
        // Update the player details for picked color & Update on server also
        GameSparksManager.Instance.GSPlayerDetailsLocal.SetRGBMaterialColor0to1(ColorPicker.PUBLIC_GetColor().r, ColorPicker.PUBLIC_GetColor().g, ColorPicker.PUBLIC_GetColor().b);
        GameSparksManager.Instance.StartNewRTSession(tempRTSessionInfo);
    }

    private void OnMatchmakingError(MatchmakingResponse obj)
    {
        UnblockInput();
        MatchStatusText.text = obj.Errors.ToString();
    }

    private void OnMatchNotFound(MatchNotFoundMessage obj)
    {
        UnblockInput();
        MatchStatusText.text = "No matches found :(";
    }

    private void BlockInput()
    {
        FindMatchButton.interactable = false;
    }

    private void UnblockInput()
    {
        FindMatchButton.interactable = true;
    }

    public void ChangePlayerColor()
    {
        mPlayerHasChangedColor = true;
        ColorPicker.PUBLIC_SetColor(MainMenuPlayerPrefab.GetComponent<MeshRenderer>());
        GameSparksManager.Instance.GSPlayerDetailsLocal.
            SetRGBMaterialColor0to1(ColorPicker.PUBLIC_GetColor().r, ColorPicker.PUBLIC_GetColor().g, ColorPicker.PUBLIC_GetColor().b);
    }

}
