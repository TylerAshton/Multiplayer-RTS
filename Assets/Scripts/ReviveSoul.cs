using UnityEngine;

public class ReviveSoul : MonoBehaviour, IInteractable
{
    public InteractionPopUp InteractionPopUp => interactionPopUp;
    [SerializeField] private InteractionPopUp interactionPopUp;
    [SerializeField] ReviveColourChange reviveColourChange;

    public void Interact()
    {
        Debug.Log("Reviving soul...");
    }

    public void ShowProgress(float _progress)
    {
        reviveColourChange.SetParticleColour(_progress);
    }
}
