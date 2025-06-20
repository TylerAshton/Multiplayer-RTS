using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class BulletProjectile : NetworkBehaviour, IDestructible, IFaction
{
    private const float LingerTime = 0.01f;
    [SerializeField] private float detectionRange = 0.1f;
    [SerializeField] float speed = 10f;
    [SerializeField] private float damage = 1f;
    [SerializeField] string friendlyTag;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private GameObject deathVFX;
    private GameObject bulletVFX;
    [SerializeField] private float lifeTime = 5f;
    private float destroyAtTime = Mathf.Infinity;
    NetworkObject networkObject;
    private bool isDead = false;
    private MeshRenderer meshRenderer;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 posLastFrame;
    private Vector3[] corners = new Vector3[8];

    private float bulletVFXScale = 1f;
    private float deathVFXScale = 1f;

    private Faction faction = Faction.None;
    public Faction Faction { get => faction; set => faction = value; }

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
        if (lifeTime > 0)
        {
            destroyAtTime = Time.fixedTime + lifeTime;
        }

        CalculateCorners();
    }

    /// <summary>
    /// Applies the projectile stats to the bullet, this is used to set the stats of the bullet when it is instantiated
    /// </summary>
    /// <param name="_projectileStats"></param>
    public void ApplyProjectileStats(ProjectileStats _projectileStats)
    {
        if (_projectileStats == null)
        {
            Debug.LogError("ProjectileStats is null");
            return;
        }

        if (!_projectileStats.IsValid())
        {
            Debug.LogError("ProjectileStats is not valid, check the console for more information");
            return;
        }

        detectionRange = _projectileStats.DetectionRange;
        speed = _projectileStats.Speed;
        damage = _projectileStats.Damage;
        lifeTime = _projectileStats.LifeTime;
        bulletVFX = _projectileStats.BulletVFX;
        bulletVFXScale = _projectileStats.BulletVFXScale;
        deathVFX = _projectileStats.DeathVFX;
        deathVFXScale = _projectileStats.DeathVFXScale;

        SpawmBulletVFXRpc();
    }

    [Rpc(SendTo.Everyone)]
    public void SpawmBulletVFXRpc()
    {
        // TODO: Check if VFX is in networked prefab pool,

        if (bulletVFX == null)
        {
            Debug.LogError("Attempted to spawn bullet vfx when it's null!");
            return;
        }

        if (bulletVFXScale >= 0)
        {
            Debug.LogError($"Bullet VFX Scale can't be zero or negative: {bulletVFXScale}");
            return;
        }
        else
        {
            GameObject spawnedVfx = Instantiate(bulletVFX, transform);
            spawnedVfx.transform.localScale *= bulletVFXScale;
        }
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
                /*if (hit.collider.gameObject.tag == friendlyTag) // This is no longer used as we now use faction enums
                {
                    continue;
                }*/

                // Skip if the hit object is part of the same faction
                if (hit.collider.TryGetComponent<IFaction>(out IFaction faction))
                {
                    if (faction.Faction == this.faction)
                    {
                        continue;
                    }
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

        posLastFrame = transform.position;





    }

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
        SpawnDeathVFX();
        networkObject.Despawn();
    }

    private void SpawnDeathVFX()
    {
        if (deathVFX == null)
        {
            Debug.LogError($"Death VFX undefined in {this.name}!");
            return;
        }

        if (deathVFXScale >= 0)
        {
            Debug.LogError($"Death VFX Scale can't be zero or negative: {deathVFXScale}");
            return;
        }

        GameObject spawnedVfx = Instantiate(deathVFX, transform.position, Quaternion.identity);
        spawnedVfx.transform.localScale *= deathVFXScale;

    }
}
