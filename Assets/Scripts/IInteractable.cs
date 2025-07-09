using UnityEngine;

public interface IInteractable
{
    public void Interact();
    void ShowProgress(float _progress) { } // This makes it so that it's optional to implement

    InteractionPopUp InteractionPopUp { get; }


}
