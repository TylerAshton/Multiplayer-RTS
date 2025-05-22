using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum ConstructionState
{
    Ready,
    Used
}


public class ConstructionPad : Unit, IConstructionPad
{
    private MeshRenderer meshRenderer;
    private Collider collider;
    private ConstructionState constructionState;
    private Building occupiedBuilding;
    public Building OccupiedBuilding => occupiedBuilding;

    ConstructionPad IConstructionPad.ConstructionPad => this;

    public Animator Animator => throw new System.NotImplementedException();

    public Transform Transform => transform;

    private AbilityPositionManager abilityPositionManager;

    public IReadOnlyDictionary<AbilityPosition, Transform> AbilityPositions => abilityPositionManager.AbilityPositions;

    public EffectManager EffectManager => throw new System.NotImplementedException();

    public IFaction IFaction => this;

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
        if (!TryGetComponent<AbilityPositionManager>(out abilityPositionManager))
        {
            Debug.LogError("AbilityPositionManager is required for ConstructionPad"); // TODO: Make all of these use gettype
        }
    }

    /// <summary>
    /// Hides the build pad from the player and sets it to not selectable while also deselecting the unit.
    /// </summary>
    public void HideBuildPad()
    {
        HidebuildPadClientRpc();
        RTSPlayer.instance.UnitManager.DeselectUnit(this);
        SetIsSelectable(false);
    }

    /// <summary>
    /// Unhides the build pad from the player and sets it to selectable.
    /// </summary>
    public void ShowBuildPad()
    {
        ShowbuildPadClientRpc();
        SetIsSelectable(true);
    }

    [ClientRpc]
    private void HidebuildPadClientRpc()
    {
        meshRenderer.enabled = false;
        collider.enabled = false;
    }

    [ClientRpc]
    private void ShowbuildPadClientRpc() // TODO: Build pads are only supposed to be visible for the amalgam player in the future
    {
        meshRenderer.enabled = true;
        collider.enabled = true;
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
