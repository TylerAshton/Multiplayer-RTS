using UnityEngine;

enum AbilityState
{
    Ready,
    Casting,
    Cooldown
}


public class AbilityManager : MonoBehaviour
{
    AbilityState abilityState = AbilityState.Ready;

    [SerializeField] private Ability primaryAbility;


}
