using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class BulletProjectile : NetworkBehaviour, IDestructible
{
    private const float LingerTime = 0.01f;
    [SerializeField] private float detectionRange = 0.1f;
    [SerializeField] float speed = 10f;
    [SerializeField] private float damage = 1f;
    [SerializeField] string friendlyTag;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private GameObject deathVFX;
    private float destroyAtTime = Mathf.Infinity;
    NetworkObject networkObject;
    private bool isDead = false;
    private MeshRenderer meshRenderer;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 posLastFrame;
    private Vector3[] corners = new Vector3[8];

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


        posLastFrame = transform.position;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Bullet spawned");

        CalculateCorners();
    }

    /// <summary>
    /// Calculates the corner extends of the bullet which are used for hit detection
    /// </summary>
    private void CalculateCorners()
    {
        Bounds bounds = meshRenderer.bounds;

        float radius = Mathf.Max(bounds.extents.x, bounds.extents.z);

        for (int i = 0; i < 12; i++)
        {
            float angle = i * (360f / 12) * Mathf.Deg2Rad;

            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        }
    }

    public void LaunchProjectile(Vector3 _direction)
    {
        if (!IsServer)
        {
            Debug.Log("Bullets can only be launched by the Server");
            return;
        }
        moveDirection = _direction;
        transform.rotation = Quaternion.LookRotation(moveDirection);
        SetDirectionClientRpc(moveDirection);
    }

    [ClientRpc]
    private void SetDirectionClientRpc(Vector3 _direction)
    {
        moveDirection = _direction;
    }



    // Update is called once per frame
    void Update()
    {
        // Client side bullet movement as it's deterministic
        MoveProjectile();

        if (!IsServer) return;

        

        // Lifetimer Check
        if (destroyAtTime < Time.fixedTime)
        {
            DestroyObject();
            return;
        }

        if (!isDead)
        {
            HitDetection();
        }
    }

    private void MoveProjectile()
    {
        if (moveDirection == Vector3.zero)
        {
            Debug.LogError("Direction isn't set, use LaunchProjectile() after instantiating a projectile");
        }

        transform.position += moveDirection * speed * Time.deltaTime;
    }


    private void OnDrawGizmos()
    {
/*        Gizmos.color = Color.red;
        Vector3 rayDirection = moveDirection.normalized * detectionRange;


        Bounds bounds = meshRenderer.bounds;

        float radius = Mathf.Max(bounds.extents.x, bounds.extents.z);

        for (int i = 0; i < 12; i++)
        {
            float angle = i * Mathf.PI * 2f / 12;

            Vector3 localOffset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
            Vector3 worldStart = transform.position + transform.rotation * localOffset;

            worldStart = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;

            Gizmos.DrawRay(worldStart, rayDirection);

        }*/

    }

    /// <summary>
    /// Detects if we've gone through anything after our last movement by taking the current 
    /// position and the position at the end of the last frame and performing a raycast
    /// </summary>
    private void HitDetection()
    {
        if (!IsServer) return;

        Vector3 directionToLastPos = (posLastFrame - transform.position).normalized;
        float distanceToLastPos = Vector3.Distance(transform.position, posLastFrame);

        foreach(Vector3 corner in corners)
        {
            if (Physics.Raycast(transform.position + corner, directionToLastPos, out RaycastHit hit, distanceToLastPos, layerMask))
            {
                if (hit.collider.gameObject.tag == friendlyTag)
                {
                    continue;
                }

                // Example: Damage logic
                if (hit.collider.TryGetComponent(out Health health))
                {
                    health.Damage(damage);
                }

                StartDespawn();
                return;
            }
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
        if (isDead)
        {
            return;
        }

        destroyAtTime = Time.fixedTime + LingerTime;
        isDead = true;

    }

    [ClientRpc]
    private void HideMeClientRpc()
    {
        meshRenderer.enabled = false;
    }

    public void DestroyObject()
    {
        Debug.Log("Killing bullet");
        if (deathVFX)
        {
            Instantiate(deathVFX, transform.position, Quaternion.identity);
        }
        networkObject.Despawn();
    }
}
