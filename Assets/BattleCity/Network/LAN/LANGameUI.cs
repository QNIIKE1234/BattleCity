using UnityEngine;
using Mirage;

public class LANGameUI : MonoBehaviour
{
    [Header("Mirage")]
    public NetworkManager networkManager;

    [Header("LAN")]
    public LanBroadcaster broadcaster;
    public LanListener listener;

    public GameObject MenuPanel;
    public GameObject RoomListPanel;
    public GameObject MenuUIPanel;

    public GameObject GamePlayUIPanel;

    public void StartHost()
    {
        if (networkManager.IsNetworkActive)
            return;

        //Host = Server + Client
        networkManager.Server.StartServer(networkManager.Client);
        //networkManager.Client.Connect();

        broadcaster.enabled = true;   
        listener.enabled = false;     

        GamePlayUIPanel.SetActive(true);
    }


     //Join LAN

    public void JoinLAN()
    {
        if (networkManager.IsNetworkActive)
            return;

        broadcaster.enabled = false;
        listener.enabled = true;


    }

 
    //Join Room

    public void JoinRoom(string ip, ushort port)
    {
        if (networkManager.IsNetworkActive)
            return;

        listener.enabled = false;
        broadcaster.enabled = false;
        MenuPanel.SetActive(false);
        RoomListPanel.SetActive(false);
        MenuUIPanel.SetActive(false);
        GamePlayUIPanel.SetActive(true);

        networkManager.Client.Connect(ip, port);
    }


}
