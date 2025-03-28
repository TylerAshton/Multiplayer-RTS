using UnityEngine;

/// <summary>
/// Weapon handler base class
/// </summary>
public abstract class Weapon : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Uses the weapon's attack, for example shoots the gun
    /// </summary>
    public abstract void Attack();

    /// <summary>
    /// Returns a bool if the weapon is ready to attack, for example if it's on cooldown
    /// </summary>
    /// <returns></returns>
    public abstract bool CanAttack();


}
