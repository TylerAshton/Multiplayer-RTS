using UnityEngine;

/// <summary>
/// This class manages hitboxes for abilities, usually projections. But should be modular enough to handle any hitbox type.
/// </summary>
public class HitboxManager : MonoBehaviour
{
    private HitboxStats hitboxStats;
    public void Init(HitboxStats _hitboxStats)
    {
        if (_hitboxStats == null)
        {
            Debug.LogError($"{nameof(_hitboxStats)} is null. Cannot initialize {GetType().Name} in gameobject - {gameObject.name}!.");
            return;
        }
    }
}
