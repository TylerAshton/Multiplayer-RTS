using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.UI.GridLayoutGroup;

public class CleaveMeleeWeapon : Weapon
{
    [SerializeField] float maxAngle = 40;
    [SerializeField] int raycastResolution = 4;
    [SerializeField] float range = 4;
    [SerializeField] LayerMask layerMask;
    [SerializeField] string friendlyTag;
    [SerializeField] float damage = 1;
    public override void Attack()
    {
        SwingCleaveServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SwingCleaveServerRpc()
    {
        if (TryHit(transform.forward))
        {
            return;
        }

        for (float i = 1; i < raycastResolution; i++)
        {
            float angle = (i / raycastResolution) * maxAngle;
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
            if (TryHit(direction))
            {
                return;
            }
            direction = Quaternion.AngleAxis(-angle, Vector3.up) * transform.forward;
            if (TryHit(direction))
            {
                return;
            }
        }
    }

    private bool TryHit(Vector3 _direction)
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, range, layerMask))
        {
            if (hit.collider.gameObject.tag != friendlyTag)
            {
                Debug.Log($"Glave hit {hit.collider.name} at {hit.point}");

                // Example: Damage logic
                if (hit.collider.TryGetComponent(out Health health))
                {
                    health.Damage(damage);
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Debug attack function for testing purposes only
    /// </summary>
    /// <param name="context"></param>
    public void INPUTAttack(InputAction.CallbackContext context)
    {
        if (!context.started)
        {
            return;
        }
        Attack();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * range);

        for(float i = 1; i < raycastResolution; i++)
        {
            float angle = (i / raycastResolution) * maxAngle;
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
            Gizmos.DrawRay(transform.position, direction * range);
            direction = Quaternion.AngleAxis(-angle, Vector3.up) * transform.forward;
            Gizmos.DrawRay(transform.position, direction * range);
            
        }
    }

    public override bool CanAttack()
    {
        throw new System.NotImplementedException();
    }
}
