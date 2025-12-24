using Mirage;
using UnityEngine;
using UnityEngine.InputSystem;

public class TankController : NetworkBehaviour
{
    [Header("Move")]
    public float speed = 5f;

    [Header("Bullet")]
    public int maxBullet = 12;
    public GameObject bulletPrefab;
    public float fireCooldown = 0.3f;

    [Header("Special")]
    public int maxSpecialPoint = 100;
    public int specialGainPerUse = 2;
    public GameObject specialBulletPrefab;

    public Transform firePoint;
    public GameObject Tank;

    [SyncVar(hook = nameof(OnBulletChanged))]
    int bulletCount;

    [SyncVar(hook = nameof(OnSpecialChanged))]
    int specialPoint;

    Vector2 cachedInput;
    float lastShootTime;

    Rigidbody2D rb;
    Collider2D myCollider;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        if (IsServer)
        {
            bulletCount = maxBullet;
            specialPoint = 20;
        }

        TryBindUI();
    }

    void TryBindUI()
    {
        if (IsLocalPlayer)
        {

         GameManager.Instance.playerUIManager.OnReload += OnReloadPressed;
         GameManager.Instance.playerUIManager.OnSpecial += OnSpecialPressed;

         GameManager.Instance.playerUIManager.UpdateBulletCount(bulletCount, maxBullet);
         GameManager.Instance.playerUIManager.UpdateSpecialCount(specialPoint, maxSpecialPoint);
        }

    }

    void Update()
    {
        if (!IsLocalPlayer) return;

        Vector2 input = Vector2.zero;
        if (Keyboard.current.aKey.isPressed) input.x = -1;
        if (Keyboard.current.dKey.isPressed) input.x = 1;
        if (Keyboard.current.wKey.isPressed) input.y = 1;
        if (Keyboard.current.sKey.isPressed) input.y = -1;

        CmdSetInput(input);

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CmdShootNormal();
        }
    }

    void FixedUpdate()
    {
        if (!IsServer) return;

        Vector2 move = cachedInput * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        if (cachedInput != Vector2.zero)
        {
            float angle = Mathf.Atan2(cachedInput.y, cachedInput.x) * Mathf.Rad2Deg;
            rb.MoveRotation(angle - 90f);
        }
    }

    [ServerRpc]
    void CmdSetInput(Vector2 input)
    {
        cachedInput = input.normalized;
    }

    [ServerRpc]
    void CmdShootNormal()
    {
        if (!GameManager.Instance.IsGameStarted) return;
        if (Time.time - lastShootTime < fireCooldown) return;
        if (bulletCount <= 0) return;

        lastShootTime = Time.time;
        //bulletCount--;

        SpawnBullet(bulletPrefab);
    }

    void OnReloadPressed()
    {
        if (!IsLocalPlayer) return;
        if (!GameManager.Instance.IsGameStarted) return;

        GameManager.Instance.OnReload();
        CmdReload();
    }

    [ServerRpc]
    void CmdReload()
    {
        if (bulletCount >= maxBullet) return;
        bulletCount++;
    }

    void OnSpecialPressed()
    {
        if (!IsLocalPlayer) return;
        if (!GameManager.Instance.IsGameStarted) return;

        GameManager.Instance.OnReload();
        CmdRequestSpecial();
    }

    [ServerRpc]
    void CmdRequestSpecial()
    {
        if (specialPoint < maxSpecialPoint)
        {
            specialPoint += specialGainPerUse;
            return;
        }

        specialPoint = 0;
        SpawnBullet(specialBulletPrefab);
    }

    [Server]
    void SpawnBullet(GameObject prefab)
    {
        if (prefab == null || firePoint == null) return;

        Vector3 spawnPos = firePoint.position + firePoint.up * 0.6f;
        GameObject bullet = Instantiate(prefab, spawnPos, firePoint.rotation);

        var bulletCol = bullet.GetComponent<Collider2D>();
        if (bulletCol && myCollider)
            Physics2D.IgnoreCollision(bulletCol, myCollider, true);

        ServerObjectManager.Spawn(bullet);
    }

    [Server]
    public void ResetPosition()
    {
        Tank.transform.position = GetRandomSafePos();
        cachedInput = Vector2.zero;
    }

    Vector3 GetRandomSafePos()
    {
        Vector2 min = new Vector2(-8, -4);
        Vector2 max = new Vector2(8, 4);

        for (int i = 0; i < 30; i++)
        {
            Vector3 p = new Vector3(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y),
                0f
            );

            if (!Physics2D.OverlapCircle(p, 0.6f))
                return p;
        }
        return Vector3.zero;
    }

    void OnBulletChanged(int oldValue, int newValue)
    {
        if (IsLocalPlayer)
        {

             GameManager.Instance.playerUIManager.UpdateBulletCount(newValue, maxBullet);
        }
    }

    void OnSpecialChanged(int oldValue, int newValue)
    {
        if (IsLocalPlayer)
        {

             GameManager.Instance.playerUIManager.UpdateSpecialCount(newValue, maxSpecialPoint);
        }
    }

}
