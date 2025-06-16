using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum ConstructionState
{
    Ready,
    Used
}


public class ConstructionPad : SelectableObject, IConstructionPad
{
    private MeshRenderer meshRenderer;
    private Collider collider;
    private ConstructionState constructionState;
    private Building occupiedBuilding;
    public Building OccupiedBuilding => occupiedBuilding;

    ConstructionPad IConstructionPad.ConstructionPad => this;

    public NetCodeAnimationManager NAnimator => throw new System.NotImplementedException();

    public Transform Transform => transform;

    private AbilityPositionManager abilityPositionManager;
    [SerializeField] public bool territoryOwned = false;

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

    protected override void Update()
    {
        base.Update();

        if (!IsServer)
        {
            return;
        }

        if (shouldDisplay())
        {
            if (!IsSelectable)
            {
                ShowBuildPad();
            }
        }
        else
        {
            if (IsSelectable)
            {
                HideBuildPad();
            }
        }
    }

    /// <summary>
    /// Hides the build pad from the player and sets it to not selectable while also deselecting the unit.
    /// </summary>
    public void HideBuildPad()
    {
        HidebuildPadClientRpc();
        RTSPlayer.instance.UnitManager.TryDeselectUnit(this);
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

    /// <summary>
    /// Returns true if the construction pad should be visible and selectable
    /// </summary>
    /// <returns></returns>
    private bool shouldDisplay() // TODO: WTF is this
    {
        if (territoryOwned && occupiedBuilding == null)
        {
            return true;
        }

        return false;
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

    internal void SetOccupiedBuilding(GameObject _building)
    {
        if(!_building.TryGetComponent<Building>(out occupiedBuilding))
        {
            Debug.LogError("Attempted to set occupied building with an object that isn't a building");
        }
    }
}
