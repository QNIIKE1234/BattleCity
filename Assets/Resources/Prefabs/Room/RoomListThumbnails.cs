using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomListThumbnails : MonoBehaviour
{
    public TextMeshProUGUI roomNameText;
    public Button joinButton;

    string ip;
    ushort port;

    public void Setup(string roomName, string ip, ushort port, System.Action<string, ushort> onJoin)
    {
        this.ip = ip;
        this.port = port;

        roomNameText.text = roomName;

        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(() =>
        {
            onJoin?.Invoke(ip, port);
        });
    }
}
