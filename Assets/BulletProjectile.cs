using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class BulletProjectile : NetworkBehaviour
{
    private const float LingerTime = 0.01f;
    [SerializeField] private float detectionRange = 0.1f;
    [SerializeField] float speed = 10f;
    [SerializeField] private float damage = 1f;
    [SerializeField] string friendlyTag;
    [SerializeField] private LayerMask layerMask;
    private float destroyAtTime = Mathf.Infinity;
    NetworkObject networkObject;
    private bool isDead = false;
    private MeshRenderer meshRenderer;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 posLastFrame;

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


    }

    public void LaunchProjectile(Vector3 _direction)
    {
        if (!IsServer)
        {
            Debug.Log("Bullets can only be launched by the Server");
            return;
        }
        moveDirection = _direction;
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
            Debug.Log("Killing bullet");
            networkObject.Despawn();
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
        Gizmos.color = Color.red;
        Vector3 rayStart = transform.position;
        Vector3 rayDirection = moveDirection.normalized * detectionRange;

        Gizmos.DrawRay(rayStart, rayDirection);
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

        if (Physics.Raycast(transform.position, directionToLastPos, out RaycastHit hit, distanceToLastPos, layerMask))
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

            StartDespawn();
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

    private void DestroyMe()
    {
        Destroy(gameObject);
    }
}
