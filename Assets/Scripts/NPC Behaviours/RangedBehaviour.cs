using UnityEngine;

[CreateAssetMenu(fileName = "New RangedBehaviour", menuName = "NPCBehaviours/RangedBehaviour", order = 1)]
public class RangedBehaviour : NPCBehaviour
{
    private UnitTargettingManager targettingManager;
    private IAbilityUser abilityUser;
    public override void Init(Unit _unit)
    {
        base.Init(_unit);

        if (!npc.IsServer)
        {
            Debug.LogError("Client attempted to initialize NPC behaviour");
            return;
        }
        if (!TryGetComponent<UnitTargettingManager>(out targettingManager))
        {
            Debug.LogError($"{nameof(UnitTargettingManager)} is required for {GetType().Name} on gameobject: {gameObject.name}");
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

    private void TryAttackTarget(NPC _npc)
    {
        if (!abilityUser.CastTarget)
        {
            return;
        }

        // TODO: Impliment checking if target is in line of sight

        _npc.AbilityManager.TryCastAbility(0);
    }

    private void UpdateRotation(NPC _npc)
    {
        if (!_npc.IsServer)
        {
            Debug.LogError("Client attempted to update rotation for NPC");
            return;
        }

        if (abilityUser.CastTarget != null)
        {
            _npc.Transform.rotation = Quaternion.Lerp(_npc.Transform.rotation, Quaternion.LookRotation(abilityUser.CastTarget.position - _npc.Transform.position), Time.deltaTime);
        }

        else if (_npc.Agent.velocity.magnitude > 0.1f)
        {
            _npc.Transform.rotation = Quaternion.Lerp(_npc.Transform.rotation, Quaternion.LookRotation(_npc.Agent.velocity.normalized), Time.deltaTime);
        }
    }
}
