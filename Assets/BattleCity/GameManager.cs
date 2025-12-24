using Mirage;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public NetworkManager networkManager;
    public TankSpawner tankSpawner;

    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI resultText;

    private bool gameRunning;
    private bool gameEnded;

    public AudioSource sound;

    public bool IsGameStarted => gameRunning;

    void Awake()
    {
        Instance = this;
        resultText.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!IsServer) return;

        if (!gameRunning && networkManager.Server.AllPlayers.Count == 2)
        {
            gameRunning = true;
            StartCoroutine(GameLoop());
        }
    }

    IEnumerator GameLoop()
    {
        RpcCountdown("3"); yield return new WaitForSeconds(1f);
        RpcCountdown("2"); yield return new WaitForSeconds(1f);
        RpcCountdown("1"); yield return new WaitForSeconds(1f);
        RpcCountdown("GO"); yield return new WaitForSeconds(0.5f);
        RpcHideCountdown();
    }

    // ================= GAME OVER =================

    [Server]
    public void ServerTankDead(TankHealth deadTank)
    {
        if (gameEnded) return;
        gameEnded = true;

        var winner = FindObjectsOfType<TankHealth>()
            .FirstOrDefault(t => t != deadTank);

        if (winner != null)
        {
         
            RpcShowResult(winner.Identity);
        }

        StartCoroutine(ResetGame());
    }

    IEnumerator ResetGame()
    {
        yield return new WaitForSeconds(3f);

        foreach (var tank in FindObjectsOfType<TankHealth>())
        {
            Vector3 pos = tankSpawner.GetRandomSafePosPublic();
            tank.ResetTank(pos);
        }

        RpcResetUI();
        gameEnded = false;
        gameRunning = false;
    }

    // ================= RPC =================

    [ClientRpc]
    void RpcShowResult(NetworkIdentity winnerIdentity)
    {
        resultText.gameObject.SetActive(true);
        resultText.text = winnerIdentity != null && winnerIdentity.HasAuthority
            ? "YOU WIN"
            : "GGEZ , YOU NOOB!";
    }

    [ClientRpc]
    void RpcResetUI()
    {
        resultText.gameObject.SetActive(false);
    }

    [ClientRpc]
    void RpcCountdown(string txt)
    {
        countdownText.gameObject.SetActive(true);
        countdownText.text = txt;
    }

    [ClientRpc]
    void RpcHideCountdown()
    {
        countdownText.gameObject.SetActive(false);
    }

    public void OnReload()
    {
        sound.Play();   
    }
}
