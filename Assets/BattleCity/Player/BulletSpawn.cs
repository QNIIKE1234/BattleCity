using Mirage;
using UnityEngine;

public class BulletSpawn : NetworkBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;
    public int damage = 1;

    Vector3 dir;
    Collider2D bulletCol;
    bool initialized;

    void Awake()
    {
        bulletCol = GetComponent<Collider2D>();
    }

    void Start()
    {
        if (!IsServer) return;

        if (!initialized)
        {
            dir = transform.up;
            Invoke(nameof(ServerDestroy), lifeTime);
        }
    }

    // Server
    public void Init(Vector3 direction, Collider2D ownerCollider)
    {
        if (!IsServer) return;

        initialized = true;
        dir = direction.normalized;

        if (bulletCol && ownerCollider)
            Physics2D.IgnoreCollision(bulletCol, ownerCollider, true);

        CancelInvoke();
        Invoke(nameof(ServerDestroy), lifeTime);
    }

    void FixedUpdate()
    {
        if (!IsServer || dir == Vector3.zero) return;
        transform.position += dir * speed * Time.fixedDeltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Tank"))
        {
            if (other.TryGetComponent<TankHealth>(out var hp))
                hp.TakeDamage(damage, dir);

            ServerDestroy();
        }
        else if (other.CompareTag("Wall"))
        {
            ServerDestroy();
        }
    }

    void ServerDestroy()
    {
        CancelInvoke();
        if (Identity && Identity.IsServer)
            Identity.ServerObjectManager.Destroy(gameObject);
    }
}
