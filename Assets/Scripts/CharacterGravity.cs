using UnityEngine;
using UnityEngine.AI;

public class CharacterGravity : MonoBehaviour
{
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundDistance = 0.1f;
    [SerializeField] private LayerMask groundMask;

    float fallSpeed = 0;
    private Collider colliderComp;
    private NavMeshAgent agent;

    private void Awake()
    {
        if (!TryGetComponent<Collider>(out colliderComp))
        {
            Debug.LogError($"{nameof(Collider)} is required for {GetType().Name}");
        }
        if (!TryGetComponent<NavMeshAgent>(out agent))
        {
            Debug.LogError($"{nameof(NavMeshAgent)} is required for {GetType().Name}");
        }
    }

    private void Update()
    {
        TryApplyGravity();
    }

    private void TryApplyGravity()
    {
        if (IsGrounded())
        {
            return;
        }

        Vector3 movement = Vector3.down * gravity * Time.deltaTime;
        agent.Move(movement);
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(GetFeet(), groundDistance, groundMask);
    }

    /// <summary>
    /// Returns a Vector3 of the lowest point of the object in the centre
    /// </summary>
    /// <returns></returns>
    public Vector3 GetFeet()
    {
        Bounds bounds = colliderComp.bounds;

        Vector3 lowestPoint = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

        return lowestPoint;
    }
}
