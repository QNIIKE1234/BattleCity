using Mirage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankSpawner : NetworkBehaviour
{
    public NetworkManager networkManager;
    public GameObject tankPrefabHost;
    public GameObject tankPrefabClient;

    [Header("Random Spawn Area")]
    public Vector2 min = new Vector2(-8, -4);
    public Vector2 max = new Vector2(8, 4);
    public float checkRadius = 0.6f;
    public GameObject bulletPrefab;

    private HashSet<INetworkPlayer> spawnedPlayers = new HashSet<INetworkPlayer>();

    void Start()
    {
        if (networkManager.Client != null && networkManager.Client.Active)
        {
            networkManager.ClientObjectManager.RegisterPrefab(
                tankPrefabHost.GetComponent<NetworkIdentity>());

            networkManager.ClientObjectManager.RegisterPrefab(
                tankPrefabClient.GetComponent<NetworkIdentity>());

            networkManager.ClientObjectManager.RegisterPrefab(
                bulletPrefab.GetComponent<NetworkIdentity>());
        }
    }

    void OnEnable()
    {
        if (networkManager.Server != null)
            networkManager.Server.Authenticated.AddListener(OnAuthenticated);
    }

    void OnDisable()
    {
        if (networkManager.Server != null)
            networkManager.Server.Authenticated.RemoveListener(OnAuthenticated);
    }

    void OnAuthenticated(INetworkPlayer player)
    {
        if (!networkManager.Server.Active) return;
        if (spawnedPlayers.Contains(player)) return;

        spawnedPlayers.Add(player);
        StartCoroutine(WaitSceneReadyAndSpawn(player));
    }

    IEnumerator WaitSceneReadyAndSpawn(INetworkPlayer player)
    {
        while (!player.SceneIsReady)
            yield return null;

        Vector3 pos = GetRandomSafePos();

        GameObject tankPrefab = player.IsHost
            ? tankPrefabHost
            : tankPrefabClient;

        GameObject tank = Instantiate(tankPrefab, pos, Quaternion.identity);
        NetworkIdentity identity = tank.GetComponent<NetworkIdentity>();

        networkManager.ServerObjectManager.AddCharacter(player, identity);
    }

    // ===== ของเดิม (private) =====
    public Vector3 GetRandomSafePos()
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 p = new Vector3(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y),
                0f
            );

            if (!Physics2D.OverlapCircle(p, checkRadius))
                return p;
        }
        return Vector3.zero;
    }

    // ===== อันนี้เพิ่มเพื่อให้ GameManager เรียกได้ =====
    public Vector3 GetRandomSafePosPublic()
    {
        return GetRandomSafePos();
    }

    [Server]
    public void ResetSpawner()
    {
        spawnedPlayers.Clear();
    }
}
