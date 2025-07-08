using UnityEngine;

public class ReviveSoul : MonoBehaviour, IInteractable
{
    public InteractionPopUp InteractionPopUp => interactionPopUp;
    [SerializeField] private InteractionPopUp interactionPopUp;

    public void Interact()
    {
        Debug.Log("Reviving soul...");
    }

    public void ShowProgress(float _progress)
    {
        Debug.Log($"Revive progress: {_progress * 100}%");
    }
}
