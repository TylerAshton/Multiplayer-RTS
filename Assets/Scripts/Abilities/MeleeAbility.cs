using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Ability", menuName = "Abilities/Melee")]
public class MeleeAbility : Ability
{
    [SerializeField] private float angleDegrees = 90f;
    [SerializeField] private float range = 4f;
    [SerializeField] private float damage = 1f;

    public override void Activate(GameObject _user, Animator _animator)
    {
        _animator.SetTrigger($"{animationTrigger}");
    }

    public override void DebugDrawing(GameObject _user, List<Transform> _abilityPositions)
    {
        Gizmos.color = Color.yellow;
        Vector3 forward = _user.transform.forward;

        Vector3 leftSide = Quaternion.AngleAxis(angleDegrees / 2, Vector3.up) * forward;
        leftSide = _user.transform.position + leftSide * range;
        Vector3 rightSide = Quaternion.AngleAxis(-angleDegrees / 2, Vector3.up) * forward;
        rightSide = _user.transform.position + rightSide * range;


        Gizmos.DrawLine(_user.transform.position, rightSide);
        Gizmos.DrawLine(_user.transform.position, leftSide);
    }

    /// <summary>
    /// Function called when the animation reaches the peak of its swing
    /// </summary>
    /// <param name="_user"></param>
    public override void OnUse(GameObject _user, List<Transform> _abilityPositions)
    {
        Vector3 origin = _user.transform.position;
        Vector3 forward = _user.transform.forward;
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
                }
            }
        }
    }

}