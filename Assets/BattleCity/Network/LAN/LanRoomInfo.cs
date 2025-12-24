using UnityEngine;

[System.Serializable]
public class LanRoomInfo
{
    public string ip;
    public int port;
    public string roomName;
    public float lastSeen;

    public LanRoomInfo(string ip, int port, string roomName)
    {
        this.ip = ip;
        this.port = port;
        this.roomName = roomName;
        lastSeen = Time.time;
    }
}
