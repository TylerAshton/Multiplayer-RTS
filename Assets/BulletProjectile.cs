using Unity.Netcode;
using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class BulletProjectile : NetworkBehaviour
{
    private Rigidbody rb;

    [SerializeField] float speed = 10f;
    [SerializeField] private float damage = 1f;
    [SerializeField] string friendlyTag;
    NetworkObject networkObject;
    private bool isDying = false;
    private Collider collider;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (friendlyTag == "")
        {
            Debug.LogError("Tag isn't assigned");
        }

        if (!TryGetComponent<NetworkObject>(out networkObject))
        {
            Debug.LogError("Network object is required for BulletProjectile");
        }
        if (!TryGetComponent<Collider>(out collider))
        {
            Debug.LogError("Collider is required for BulletProjectile");
        }
        if (!TryGetComponent<MeshRenderer>(out meshRenderer))
        {
            Debug.LogError("MeshRenderer is required for BulletProjectile");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Bullet spawned");
    }

    public void Fire()
    {
        rb.linearVelocity = transform.forward * speed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("Unit")) ;
        {
            Debug.Log("Unit;");
        }
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if (other.CompareTag(friendlyTag)) // Friendly fire will not be tolerated
        {
            return;
        }
        if (other.TryGetComponent<Health>(out var _health))
        {
            _health.Damage(damage);
        }

        Debug.Log("Bullet Destroyed");
        StartDespawn();
    }

    public void StartDespawn()
    {
        if (isDying)
        {
            return;
        }

        isDying = true;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        collider.enabled = false;
        meshRenderer.enabled = false;
        //HideMeClientRpc();

        Invoke(nameof(DestroyMe), 0.2f);
    }

    [ClientRpc]
    private void HideMeClientRpc()
    {
        meshRenderer.enabled = false;
    }

    private void DestroyMe()
    {
        Destroy(gameObject);
    }
}
