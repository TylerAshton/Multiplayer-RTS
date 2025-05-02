using Unity.Netcode;
using UnityEngine;

public enum ConstructionState
{
    Ready,
    Used
}


public class ConstructionPad : Unit
{
    private MeshRenderer meshRenderer;
    private Collider collider;
    private ConstructionState constructionState;
    private Building occupiedBuilding;
    public Building OccupiedBuilding => occupiedBuilding;

    protected override void Awake()
    {
        base.Awake();

        if (!TryGetComponent<MeshRenderer>(out meshRenderer))
        {
            Debug.LogError("MeshRenderer is required for ConstructionPad");
        }
        if (!TryGetComponent<Collider>(out collider))
        {
            Debug.LogError("Collider is required for ConstructionPad");
        }
    }

    public void HideBuildPad()
    {
        meshRenderer.enabled = false;
        collider.enabled = false;
        SetIsSelectable(false);
    }

    public void ShowBuildPad()
    {
        meshRenderer.enabled = true;
        collider.enabled = true;
        SetIsSelectable(true);
    }

    public void SetConstructionState(ConstructionState _state)
    {
        constructionState = _state;
    }

    public void DestroyOccupiedBuilding()
    {
        occupiedBuilding.GetComponent<Health>().DestroyObject();
    }


}
