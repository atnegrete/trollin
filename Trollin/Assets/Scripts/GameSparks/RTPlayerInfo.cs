using UnityEngine;
using UnityEditor;

public class RTPlayerInfo
{
    public RTPlayerInfo() {
        GSPlayerDetails = new GSPlayerDetails();
    }

    public RTPlayerInfo(string _displayName, string _id, int _peerId) : this()
    {
        this.displayName = _displayName;
        this.id = _id;
        this.peerId = _peerId;
        this.GSPlayerDetails.peerId = peerId;
    }

    public string displayName;
    public string id;
    public int peerId;
    public bool isOnline;
    public GSPlayerDetails GSPlayerDetails;
}