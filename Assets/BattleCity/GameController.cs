using Mirage;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class GameController : MonoBehaviour
{
    public GameObject exitPanel;
    public NetworkManager networkManager;

    void Update()
    {
        if (Keyboard.current.escapeKey.isPressed)
        {
            if (exitPanel.activeInHierarchy)
            {

                exitPanel.SetActive(false);
            }
            else
            {
                exitPanel.SetActive(true);
            }
        }
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }
}
