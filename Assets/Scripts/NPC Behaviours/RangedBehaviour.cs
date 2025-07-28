using UnityEngine;

[CreateAssetMenu(fileName = "New RangedBehaviour", menuName = "NPCBehaviours/RangedBehaviour", order = 1)]
public class RangedBehaviour : NPCBehaviour
{
    private IAbilityUser abilityUser;
    [SerializeField] private float targettingAngle = 60f;
    public override void Init()
    {
        base.Init();

        if (!npc.IsServer)
        {
            Debug.LogError("Client attempted to initialize NPC behaviour");
            return;
        }
        if (!TryGetComponent<IAbilityUser>(out abilityUser))
        {
            Debug.LogError($"{nameof(IAbilityUser)} is required for {GetType().Name} on gameobject: {gameObject.name}");
            return;
        }


    }
    public override void Tick()
    {
        base.Tick();

        if (!npc.IsServer)
        {
            Debug.LogError("Client attempted to Update NPC behaviour");
            return;
        }

        UpdateRotation(npc);
        TryAttackTarget(npc);
    }

    private bool IsWithinCone(Vector3 _targetPos)
    {
        Vector3 contactDirection = _targetPos - transform.position;
        contactDirection.y = 0;

        Vector3 forwardDirection = transform.forward;
        forwardDirection.y = 0;

        float dot = Vector3.Dot(forwardDirection.normalized, contactDirection.normalized);

        float cosAngle = Mathf.Cos(targettingAngle * 0.5f * Mathf.Deg2Rad);

        return dot >= cosAngle;
    }

    private void TryAttackTarget(NPC _npc)
    {
        if (abilityUser.AimPoint == Vector3.zero)
        {
            return;
        }

        // Checking if they're infront of player
        if (!IsWithinCone(abilityUser.AimPoint))
        {
            return;
        }

        // TODO: Impliment checking if target is in line of sight

        _npc.AbilityManager.TryCastAbility(0, 0);
    }

    private void UpdateRotation(NPC _npc)
    {
        if (!_npc.IsServer)
        {
            Debug.LogError("Client attempted to update rotation for NPC");
            return;
        }

        if (abilityUser.AimPoint != Vector3.zero)
        {
            _npc.Transform.rotation = Quaternion.Lerp(_npc.Transform.rotation, Quaternion.LookRotation(abilityUser.AimPoint - _npc.Transform.position), Time.deltaTime);
        }

        else if (_npc.Agent.velocity.magnitude > 0.1f)
        {
            _npc.Transform.rotation = Quaternion.Lerp(_npc.Transform.rotation, Quaternion.LookRotation(_npc.Agent.velocity.normalized), Time.deltaTime);
        }
    }
}
