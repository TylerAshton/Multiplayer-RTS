using Unity.Netcode;
using UnityEngine;

public class BulletProjectile : NetworkBehaviour
{

    [SerializeField] float speed = 10f;
    [SerializeField] private float damage = 1f;
    [SerializeField] string friendlyTag;
    [SerializeField] private LayerMask layerMask;
    NetworkObject networkObject;
    private bool isDying = false;
    private MeshRenderer meshRenderer;
    private Vector3 direction = Vector3.zero;

    private void Awake()
    {

        if (friendlyTag == "")
        {
            Debug.LogError("Tag isn't assigned");
        }

        if (!TryGetComponent<NetworkObject>(out networkObject))
        {
            Debug.LogError("Network object is required for BulletProjectile");
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

    public void LaunchProjectile(Vector3 _direction)
    {
        direction = _direction;
    }

    // Update is called once per frame
    void Update()
    {
        MoveProjectile();
        HitDetection();

        if (direction == Vector3.zero)
        {
            Debug.LogError("Direction isn't set, use LaunchProjectile() after instantiating a projectile");
        }
    }

    private void MoveProjectile()
    {
        transform.position += direction * speed * Time.deltaTime;

        if (!IsServer) return;

    }

    private void HitDetection()
    {
        //if (!IsServer) return;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, 0.1f, layerMask))
        {
            if (hit.collider.gameObject.tag == friendlyTag)
            {
                return;
            }

            Debug.Log($"Bullet hit {hit.collider.name} at {hit.point}");

            // Example: Damage logic
            if (hit.collider.TryGetComponent(out Health health))
            {
                health.Damage(damage);
            }

            Destroy(gameObject);
        }



    }

/*    private void OnTriggerEnter(Collider other)
    {
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
    }*/

    private void StartDespawn()
    {
        if (isDying)
        {
            return;
        }

        isDying = true;
        GetComponent<Collider>().enabled = false;
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
