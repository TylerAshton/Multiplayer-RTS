using NUnit.Framework;
using System.Collections.Generic;
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
    [SerializeField] private LayerMask objectsLayerMask;
    [SerializeField] private LayerMask unitsOnlyLayerMask;
    [SerializeField] private GameObject deathVFX;
    private GameObject bulletVFX;
    [SerializeField] private float lifeTime = 5f;
    private float destroyAtTime = Mathf.Infinity;
    NetworkObject networkObject;
    private bool isDead = false;
    private MeshRenderer meshRenderer;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 posLastFrame;

    private float bulletVFXScale = 1f;
    private float deathVFXScale = 1f;
    private bool isAOE = false;
    private float aoeRadius = 1f;
    private int penetration = 0;
    private Faction faction = Faction.None;
    public Faction Faction { get => faction; set => faction = value; }
    private List<GameObject> hitTagets = new List<GameObject>();
    private VFXScaler vfxScaler;

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
        if (!TryGetComponent<VFXScaler>(out vfxScaler))
        {
            Debug.LogError($"VFXScaler is required for BulletProjectile");
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
    }

    /// <summary>
    /// Applies the projectile stats to the bullet, this is used to set the stats of the bullet when it is instantiated
    /// </summary>
    /// <param name="_projectileStats"></param>
    /// 
    [Rpc(SendTo.Everyone)]
    private void ApplyProjectileStatsRpc(string _projectileStatsID)
    {
        ProjectileStats _projectileStats = AbilityStatsRegistry.GetProjectileStat<ProjectileStats>(_projectileStatsID);

        

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
        isAOE = _projectileStats.IsAOE;
        aoeRadius = _projectileStats.AOERadius;
        penetration = _projectileStats.Penetration;

        SpawmBulletVFXRpc();
    }

    public void ApplyProjectileStatsWithID(string _projectileStatsID)
    {
        ApplyProjectileStatsRpc(_projectileStatsID);
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

        if (bulletVFXScale <= 0)
        {
            Debug.LogError($"Bullet VFX Scale can't be zero or negative: {bulletVFXScale}");
            return;
        }
        else
        {
            GameObject spawnedVfx = Instantiate(bulletVFX, transform);
            //spawnedVfx.transform.localScale *= bulletVFXScale;
            vfxScaler.ScaleParticles(bulletVFXScale, spawnedVfx);
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

    public void LaunchProjectileAtTarget(Vector3 _targetPos)
    {
        Vector3 direction = _targetPos - transform.position;

        //direction.y = 0f; 

        direction = direction.normalized;


        LaunchProjectile(direction);
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
            ForwardHitDetection();
            TunnelHitDetection();
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
        if (isAOE)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, aoeRadius);
        }

    }

    /// <summary>
    /// Raycasts fprward in the direction of the projectile to detect hits with objects.
    /// </summary>
    private void ForwardHitDetection()
    {
        if (!IsServer)
        {
            Debug.Log($"{nameof(ForwardHitDetection)} can only be performed on the server!");
            return;
        }

        // Perform a raycast in the direction of the projectile
        if (Physics.Raycast(transform.position, moveDirection, out RaycastHit hit, detectionRange, objectsLayerMask))
        {
            HandleHit(hit.collider);
        }
    }

    /// <summary>
    /// Detects if we've gone through anything after our last movement by taking the current 
    /// position and the position at the end of the last frame and performing a raycast.
    /// if we hit something, we will handle the hit.
    /// </summary>
    private void TunnelHitDetection()
    {
        if (!IsServer)
        {
            Debug.Log($"{nameof(TunnelHitDetection)} can only be performed on the server!");
            return;
        }

        Vector3 directionFromLastPos = (transform.position - posLastFrame).normalized;
        float distanceToLastPos = Vector3.Distance(transform.position, posLastFrame); // It should be last pos ---> currentpos

        if (Physics.Raycast(posLastFrame, directionFromLastPos, out RaycastHit hit, distanceToLastPos, objectsLayerMask))
        {
            HandleHit(hit.collider);
        }

        posLastFrame = transform.position;
    }

    private void HandleHit(Collider _hitCollider)
    {
        if (!IsServer)
        {
            Debug.LogError("HandleHit can only be performed on the server!");
            return;
        }

        // If we've hit this before leave it.
        if (hitTagets.Contains(_hitCollider.gameObject))
        {
            return;
        }

        if (_hitCollider.TryGetComponent<IFaction>(out IFaction faction))
        {
            if (faction.Faction == this.faction)
            {
                return;
            }
        }
        TryDamage(_hitCollider);

        AOEHitDetection(_hitCollider);

        if (penetration > 0)
        {
            penetration--;
            hitTagets.Add(_hitCollider.gameObject);
            SpawnDeathVFXRpc(); // TODO: Perhaps use a different vfx than death 
        }
        else
        {
            StartDespawn();
        }
    }

    /// <summary>
    /// If collider is not a friednly and has health, will apply damage to it.
    /// </summary>
    /// <param name="_hitCollider"></param>
    private void TryDamage(Collider _hitCollider)
    {
        if (_hitCollider.TryGetComponent<IFaction>(out IFaction faction))
        {
            if (faction.Faction == this.faction)
            {
                return;
            }
        }

        // Damage logic
        if (_hitCollider.TryGetComponent(out Health health))
        {
            health.Damage(damage);
        }
    }

    /// <summary>
    /// If the projectile is an AOE projectile, this will perform hit detection for all OTHER objects within the AOE radius and apply damage to them.
    /// </summary>
    private void AOEHitDetection(Collider _exempt)
    {
        if (!IsServer)
        {
            Debug.LogError("AOE Hit Detection can only be performed on the server");
        }

        if (!isAOE)
        {
            return;
        }

        // Check sphere overlap for AOE detection
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, aoeRadius, unitsOnlyLayerMask);

        // Remove _exempt from hit colliders if exists
        if (_exempt != null)
        {
            hitColliders = System.Array.FindAll(hitColliders, collider => collider != _exempt);
        }

        foreach (Collider hitCollider in hitColliders)
        {
            TryDamage(hitCollider);
        }



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
        SpawnDeathVFXRpc();
        networkObject.Despawn();
    }

    [Rpc(SendTo.Everyone)]
    private void SpawnDeathVFXRpc()
    {
        if (deathVFX == null)
        {
            Debug.LogError($"Death VFX undefined in {this.name}!");
            return;
        }

        if (deathVFXScale <= 0)
        {
            Debug.LogError($"Death VFX Scale can't be zero or negative: {deathVFXScale}in {this.name}");
            return;
        }

        GameObject spawnedVfx = Instantiate(deathVFX, transform.position, Quaternion.identity);
        //spawnedVfx.transform.localScale *= deathVFXScale;
        vfxScaler.ScaleParticles(deathVFXScale, spawnedVfx);

    }
}
