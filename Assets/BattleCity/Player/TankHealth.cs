using Mirage;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TankHealth : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnHpChanged))]
    public int hp = 25;

    private int maxHp;
    private bool isDead;
    private bool invincible;

    public Slider hpBar;
    public GameObject ExplosionVFX;
    public GameObject ShotVFX;

    private TankController tankController;
    private Collider2D[] colliders;
    private Renderer[] renderers;

    public PlayerUIManager playerUIManager;

    void Awake()
    {
        tankController = GetComponent<TankController>();
        colliders = GetComponentsInChildren<Collider2D>(true);
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    void Start()
    {
        maxHp = hp;
        OnHpChanged(0, hp);

        ResolvePlayerUI();
    }

    void OnHpChanged(int oldHp, int newHp)
    {
        if (hpBar != null)
            hpBar.value = (float)newHp / maxHp;

        playerUIManager.UpdateUIHp(hp, maxHp);
    }

    // ================= DAMAGE =================

    [Server]
    public void TakeDamage(int dmg, Vector3 hitDir)
    {
        if (isDead || invincible) return;

        hp -= dmg;
        RpcHitVFX(false);
        StartCoroutine(IFrame());

        if (hp <= 0)
        {
            hp = 0;
            isDead = true;

            RpcHitVFX(true);
            GameManager.Instance.ServerTankDead(this);
            RpcHideTank();
        }
    }

    IEnumerator IFrame()
    {
        invincible = true;
        yield return new WaitForSeconds(0.4f);
        invincible = false;
    }

    // ================= RPC =================

    [ClientRpc]
    void RpcHitVFX(bool dead)
    {
        GameObject vfx = null;

        if (dead && ExplosionVFX != null)
            vfx = Instantiate(ExplosionVFX, transform.position, Quaternion.identity);
        else if (!dead && ShotVFX != null)
            vfx = Instantiate(ShotVFX, transform.position, Quaternion.identity);

        if (vfx != null)
            Destroy(vfx, 0.25f);
    }


    [ClientRpc]
    void RpcHideTank()
    {
        SetTankVisible(false);
    }

    // ================= RESET =================

    [Server]
    public void ResetTank(Vector3 pos)
    {
        hp = maxHp;
        isDead = false;
        invincible = false;

        RpcResetTank(pos);
    }

    [ClientRpc]
    void RpcResetTank(Vector3 pos)
    {
        transform.position = pos;
        SetTankVisible(true);

        if (hpBar != null)
            hpBar.value = 1f;
    }

    // ================= VISIBILITY =================

    void SetTankVisible(bool visible)
    {
        if (tankController != null)
            tankController.enabled = visible;

        foreach (var c in colliders)
            c.enabled = visible;

        foreach (var r in renderers)
            r.enabled = visible;
    }

    void ResolvePlayerUI()
    {
        if (!IsLocalPlayer) return;

        playerUIManager = GameObject.Find("CanvaUI")?.GetComponentInChildren<PlayerUIManager>(true);
    }
}
