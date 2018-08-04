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

    private RTSessionInfo tempRTSessionInfo;

    void Awake () {
        FindMatchButton.onClick.AddListener(FindMatch);
        MatchNotFoundMessage.Listener += OnMatchNotFound;

        // Request the players details
        // TODO
    }

    private void FindMatch()
    {
        BlockInput();
        MatchStatusText.text = "Searching for matches...";
        MatchmakingRequest request = new MatchmakingRequest();
        request.SetMatchShortCode("SC_MatchTest");
        request.SetSkill(1);
        request.Send(OnMatchmakingSuccess, OnMatchmakingError);

        // Update color preference
        var color = ColorPicker.PUBLIC_GetColor();
        LogEventRequest_UP_Player_Details playerDetailsRequest = new LogEventRequest_UP_Player_Details();
        Debug.Log(string.Format("GSM EVENT| RED {0}, G {1}, B {2}", color.r * 100, color.g * 100, color.b * 100));
        playerDetailsRequest.Set_BLUE((long)(color.b*100)).Set_GREEN((long)(color.g*100)).Set_RED((long)(color.r*100)).Send((response) =>
        {
            // TODO: Check for errors
        });
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
        foreach (GameSparks.Api.Messages.MatchFoundMessage._Participant player in _message.Participants)
        {
            sBuilder.AppendLine("Player:" + player.PeerId + " User Name:" + player.DisplayName); // add the player number and the display name to the list
        }

        tempRTSessionInfo = new RTSessionInfo(_message);

        StartGSGameSession();
    }

    private void StartGSGameSession()
    {
        // Set the player details for the game
        GSPlayerDetails playerDetails = new GSPlayerDetails();
        playerDetails.SetRGBMaterialColor0to1(ColorPicker.PUBLIC_GetColor().r, ColorPicker.PUBLIC_GetColor().g, ColorPicker.PUBLIC_GetColor().b);

        GameSparksManager.Instance.StartNewRTSession(tempRTSessionInfo, playerDetails);
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

}
