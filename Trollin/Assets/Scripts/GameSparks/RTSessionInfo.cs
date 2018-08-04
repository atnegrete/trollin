using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using GameSparks.Api.Messages;

public class RTSessionInfo
{
    private string hostURL;
    public string GetHostURL() { return this.hostURL; }
    private string acccessToken;
    public string GetAccessToken() { return this.acccessToken; }
    private int portID;
    public int GetPortID() { return this.portID; }
    private string matchID;
    public string GetMatchID() { return this.matchID; }

    private List<RTPlayerInfo> RTPlayerList = new List<RTPlayerInfo>();
    public List<RTPlayerInfo> GetRTPlayerList()
    {
        return RTPlayerList;
    }

    /// <summary>
    /// Creates a new RTSession object which is held until a new RT session is created
    /// </summary>
    /// <param name="_message">Message.</param>
    public RTSessionInfo(MatchFoundMessage _message)
    {
        portID = (int)_message.Port;
        hostURL = _message.Host;
        acccessToken = _message.AccessToken;
        matchID = _message.MatchId;
        // we loop through each participant and get their peerId and display name //
        foreach (MatchFoundMessage._Participant p in _message.Participants)
        {
            RTPlayerList.Add(new RTPlayerInfo(p.DisplayName, p.Id, (int)p.PeerId));
        }
    }
}