using System.Collections;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class LanBroadcaster : MonoBehaviour
{
    public int broadcastPort = 47777;
    public int gamePort = 7777;
    public string roomName = "LAN Room";

    UdpClient udp;
    IPEndPoint endPoint;
    Coroutine broadcastCoroutine;
    bool running;

    void OnEnable()
    {
        udp = new UdpClient();
        udp.EnableBroadcast = true;
        endPoint = new IPEndPoint(IPAddress.Broadcast, broadcastPort);

        running = true;
        broadcastCoroutine = StartCoroutine(BroadcastLoop());
    }

    IEnumerator BroadcastLoop()
    {
        while (running)
        {
            try
            {
                string msg = $"ROOM|{GetLocalIP()}|{gamePort}|{roomName}";
                byte[] data = Encoding.UTF8.GetBytes(msg);
                udp.Send(data, data.Length, endPoint);
            }
            catch
            {
                // udp ถูกปิดแล้ว → ออก loop
                yield break;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    void OnDisable()
    {
        running = false;

        if (broadcastCoroutine != null)
            StopCoroutine(broadcastCoroutine);

        udp?.Close();
        udp = null;
    }

    string GetLocalIP()
    {
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up)
                continue;

            if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                continue;

            var ipProps = ni.GetIPProperties();
            foreach (var ua in ipProps.UnicastAddresses)
            {
                if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ua.Address.ToString();
                }
            }
        }
        return "127.0.0.1";
    }
}
