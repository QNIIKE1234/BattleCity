using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class LanListener : MonoBehaviour
{
    public int listenPort = 47777;
    public float timeout = 3f;

    UdpClient udp;
    Thread thread;
    volatile bool running;

    public Dictionary<string, LanRoomInfo> rooms = new();
    ConcurrentQueue<LanRoomInfo> incomingRooms = new();

    void OnEnable()
    {
        udp = new UdpClient(listenPort);
        running = true;

        thread = new Thread(ListenLoop);
        thread.IsBackground = true;
        thread.Start();
    }

    void ListenLoop()
    {
        IPEndPoint ep = new IPEndPoint(IPAddress.Any, listenPort);

        while (running)
        {
            try
            {
                byte[] data = udp.Receive(ref ep);
                string msg = Encoding.UTF8.GetString(data);

                var split = msg.Split('|');
                if (split.Length < 4 || split[0] != "ROOM")
                    continue;

                string ip = split[1];
                int port = int.Parse(split[2]);
                string name = split[3];

                incomingRooms.Enqueue(new LanRoomInfo(ip, port, name));
            }
            catch
            {
                // socket off
                break;
            }
        }
    }

    void Update()
    {
        // thread recieve
        while (incomingRooms.TryDequeue(out var room))
        {
            string key = room.ip + ":" + room.port;
            rooms[key] = room;
        }

        // timeout
        var remove = new List<string>();
        foreach (var r in rooms)
        {
            if (Time.time - r.Value.lastSeen > timeout)
                remove.Add(r.Key);
        }

        foreach (var k in remove)
            rooms.Remove(k);
    }

    void OnDisable()
    {
        running = false;
        udp?.Close();
        thread?.Join();
    }
}
