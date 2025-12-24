using Mirage;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(NetworkTransform))]
public class TankAuthorityFix : MonoBehaviour
{
    NetworkTransform netTransform;
    NetworkIdentity identity;

    void Awake()
    {
        netTransform = GetComponent<NetworkTransform>();
        identity = GetComponent<NetworkIdentity>();

        netTransform.enabled = false;
    }

    void OnEnable()
    {
        if (identity != null)
            identity.OnAuthorityChanged.AddListener(OnAuthorityChanged);
    }

    void OnDisable()
    {
        if (identity != null)
            identity.OnAuthorityChanged.RemoveListener(OnAuthorityChanged);
    }

    void OnAuthorityChanged(bool hasAuthority)
    {
        netTransform.enabled = hasAuthority;
    }
}
