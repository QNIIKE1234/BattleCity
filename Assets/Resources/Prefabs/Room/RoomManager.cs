using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomManager : MonoBehaviour
{
    public LanListener lanListener;
    public LANGameUI lanGameUI;

    public Transform content;
    public GameObject listThumbnailPrefab;

    Dictionary<string, RoomListThumbnails> roomUIs = new();

    void Update()
    {
        RefreshRoomList();
    }

    void RefreshRoomList()
    {
        if (!this.isActiveAndEnabled)
            return;

        foreach (var room in lanListener.rooms.Values)
        {
            string key = room.ip + ":" + room.port;

            if (roomUIs.ContainsKey(key))
                continue;

            GameObject go = Instantiate(listThumbnailPrefab, content);
            var ui = go.GetComponent<RoomListThumbnails>();

            ui.Setup(
                room.roomName,
                room.ip,
                (ushort)room.port,
                lanGameUI.JoinRoom
            );

            roomUIs[key] = ui;
        }

        // ลบห้องที่หายไป
        var remove = new List<string>();
        foreach (var pair in roomUIs)
        {
            if (!lanListener.rooms.ContainsKey(pair.Key))
                remove.Add(pair.Key);
        }

        foreach (var key in remove)
        {
            Destroy(roomUIs[key].gameObject);
            roomUIs.Remove(key);
        }
    }

}
