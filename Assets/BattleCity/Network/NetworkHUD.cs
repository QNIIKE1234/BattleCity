using UnityEngine;
using Mirage;

public class NetworkHUD : MonoBehaviour
{
    public NetworkManager manager;

    public void StartHost()
    {
        manager.Server.StartServer();
        manager.Client.Connect();
    }

    public void StartClient()
    {
        manager.Client.Connect();
    }

    public void StopAll()
    {
        manager.Client.Disconnect();
        manager.Server.Stop();
    }
}
