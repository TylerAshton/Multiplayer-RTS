using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public interface IAbilityUser
{
    Transform Transform { get; }
    IReadOnlyDictionary<AbilityPosition, Transform> AbilityPositions { get; }   
    AbilityManager AbilityManager { get; }
    Vector3 AimPoint { get; }
    IFaction IFaction { get; }

    ulong OwnerID { get; }

    public void SetTarget(Collider castTarget);
    public void ClearTarget();
}
