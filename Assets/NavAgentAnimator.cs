using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class NavAgentAnimator : NetworkBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;
    private void Awake()
    {
        if (!TryGetComponent<Animator>(out animator))
        {
            Debug.LogError("Animator is required for NavAgentAnimator");
        }
        if (!TryGetComponent<NavMeshAgent>(out agent))
        {
            Debug.LogError("NavMeshAgent is required for NavAgentAnimator");
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        Vector3 movementNormalized = agent.velocity.normalized;

        UpdateAnimationParamsServerRpc(movementNormalized);
    }

    /// <summary>
    /// Updates the animator controller with the movement vector relative to the rotation
    /// </summary>
    /// <param name="_movementInput"></param>
    /// 
    [ServerRpc(RequireOwnership = false)]
    private void UpdateAnimationParamsServerRpc(Vector3 _movementInput)
    {
        if (_movementInput.sqrMagnitude < 0.001f) // Smoothing even when we're standing still
        {
            animator.SetFloat("MoveX", Mathf.Lerp(animator.GetFloat("MoveX"), 0f, 5f * Time.deltaTime));
            animator.SetFloat("MoveY", Mathf.Lerp(animator.GetFloat("MoveY"), 0f, 5f * Time.deltaTime));
            return;
        }

        Vector3 input = _movementInput.normalized;
        float relativeX = Vector3.Dot(input, transform.right); // .Dot() Exists!! 
        float relativeZ = Vector3.Dot(input, transform.forward);

        animator.SetFloat("MoveX", Mathf.Lerp(animator.GetFloat("MoveX"), relativeX, 5.0f * Time.deltaTime));
        animator.SetFloat("MoveY", Mathf.Lerp(animator.GetFloat("MoveY"), relativeZ, 5.0f * Time.deltaTime));
    }



}
