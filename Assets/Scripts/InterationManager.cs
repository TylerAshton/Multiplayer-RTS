using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class InterationManager : NetworkBehaviour
{
    IInteractable currentInteractable;
    [SerializeField] private LayerMask interactableLayerMask;
    [SerializeField] private float interactionRange;
    private bool isHolding = false; 
    private float holdTimeCounter = 0f;
    [SerializeField] private float holdTimeThreshold = 1.5f; // Time in seconds to hold for interaction



    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        SetClosestInteractable();
        OnHolding();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    public void OnInteractStart(InputAction.CallbackContext context)
    {
        if (currentInteractable == null)
        {
            Debug.LogWarning("No interactable object in range.");
            return;
        }

        if (context.started)
        {
            isHolding = true;
        }

        if (context.canceled)
        {
            ResetHold();
        }   
    }

    private void OnHolding()
    {
        if (currentInteractable == null) // Detect if we're now out of range
        {
            ResetHold();
        }

        if (!isHolding)
        {
            return;
        }

        holdTimeCounter += Time.deltaTime;
        if (holdTimeCounter >= holdTimeThreshold)
        {
            currentInteractable.Interact();
            ResetHold(); // Reset after interaction
        }
    }

    private void ResetHold()
    {
        isHolding = false;
        holdTimeCounter = 0f;
    }

    private void SetClosestInteractable()
    {
        currentInteractable = null;
        float closestDistance = float.MaxValue;

        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange, interactableLayerMask);

        foreach (Collider _hit in hits)
        {
            if (!_hit.TryGetComponent(out IInteractable interactable))
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, _hit.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentInteractable = interactable;
            }
        }
    }
}
