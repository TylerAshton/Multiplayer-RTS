using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.ProBuilder;

public class ChampionShoot : NetworkBehaviour
{
    private Vector3 mouseScreenPos;
    private Vector3 worldPosition;
    [SerializeField] private GameObject projectile;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.position, worldPosition);

    }

    public void OnPoint(InputAction.CallbackContext context)
    {
        mouseScreenPos = context.ReadValue<Vector2>();


        worldPosition = new Vector3(0, 0, 0);
        Ray r = Camera.main.ScreenPointToRay(mouseScreenPos);
        if (Physics.Raycast(r, out RaycastHit hit))
        {
            worldPosition = hit.point;
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (!IsOwner)
        {
            return;
        }

        Vector3 direction = (worldPosition - transform.position).normalized;
        direction.y = 0;

        FireBulletServerRpc(direction);        
    }

    [ServerRpc(RequireOwnership = false)]
    private void FireBulletServerRpc(Vector3 direction)
    {
        GameObject newProjectile = (GameObject)Instantiate(projectile, transform.position, Quaternion.LookRotation(direction));

        // Register over network
        NetworkObject bulletNetwork = newProjectile.GetComponent<NetworkObject>();
        bulletNetwork.Spawn();
        newProjectile.SetActive(true);

        BulletProjectile _projectile = newProjectile.GetComponent<BulletProjectile>();
        _projectile.Fire();
    }
}
