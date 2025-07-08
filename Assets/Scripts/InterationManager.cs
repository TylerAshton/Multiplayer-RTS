using UnityEngine;

public class InterationManager : MonoBehaviour
{
    IInteractable currentInteractable;
    [SerializeField] private LayerMask interactableLayerMask;
    [SerializeField] private float interactionRange;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SetClosestInteractable();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    public void Interact()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
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
