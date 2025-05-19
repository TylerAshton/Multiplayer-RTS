using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Ability", menuName = "Abilities/Melee")]
public class MeleeAbility : Ability<IAbilityUser>
{
    [SerializeField] private float angleDegrees = 90f;
    [SerializeField] private float range = 4f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private GameObject hitEffect;

    protected override void ActivateTyped(IAbilityUser _user)
    {
        _user.Animator.SetTrigger($"{AnimationTrigger}");
    }

    protected override void DebugDrawingTyped(IAbilityUser _user)
    {
        Gizmos.color = Color.yellow;
        Vector3 forward = _user.Transform.forward;

        Vector3 leftSide = Quaternion.AngleAxis(angleDegrees / 2, Vector3.up) * forward;
        leftSide = _user.Transform.position + leftSide * range;
        Vector3 rightSide = Quaternion.AngleAxis(-angleDegrees / 2, Vector3.up) * forward;
        rightSide = _user.Transform.position + rightSide * range;


        Gizmos.DrawLine(_user.Transform.position, rightSide);
        Gizmos.DrawLine(_user.Transform.position, leftSide);
    }

    /// <summary>
    /// Function called when the animation reaches the peak of its swing
    /// </summary>
    /// <param name="_user"></param>
    protected override void OnUseTyped(IAbilityUser _user)
    {
        Vector3 origin = _user.Transform.position;
        Vector3 forward = _user.Transform.forward;
        float cosAngle = Mathf.Cos(angleDegrees * 0.5f * Mathf.Deg2Rad); // Conversion of our angleDegrees to a cos for dot. 
                                                                         // We use half as the full cone is angleDegrees

        Collider[] hits = Physics.OverlapSphere(origin, range);
        foreach (var hit in hits)
        {
            Vector3 toTarget = (hit.transform.position - origin).normalized;
            float dot = Vector3.Dot(forward, toTarget);

            if (dot >= cosAngle) // If is within our cone degrees, hit
            {
                if (hit.TryGetComponent(out Health _health))
                {
                    _health.Damage(damage);
                    GameObject hitVFX = Instantiate(hitEffect, hit.transform);
                    hitVFX.GetComponent<NetworkObject>().Spawn();
                }
            }
        }
    }

}