using UnityEngine;

[CreateAssetMenu(fileName = "New RangedBehaviour", menuName = "NPCBehaviours/RangedBehaviour", order = 1)]
public class RangedBehaviour : NPCBehaviour
{
    public override void Init(NPC _npc)
    {
        if (!_npc.IsServer)
        {
            Debug.LogError("Client attempted to initialize NPC behaviour");
            return;
        }
    }
    public override void Update(NPC _npc, float deltaTime)
    {
        if (!_npc.IsServer)
        {
            Debug.LogError("Client attempted to Update NPC behaviour");
            return;
        }

        UpdateRotation(_npc);
        TryAttackTarget(_npc);
    }
    public override void OnSetTarget(NPC _npc)
    {
        if (!_npc.IsServer)
        {
            Debug.LogError("Client attempted to run OnSetTarget for NPC");
            return;
        }
    }

    public override void OnClearTarget(NPC _npc)
    {
        if (!_npc.IsServer)
        {
            Debug.LogError("Client attempted to run OnClearTarget for NPC");
            return;
        }
    }

    private void TryAttackTarget(NPC _npc)
    {
        if (!_npc.Target)
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

        if (_npc.Target != null)
        {
            _npc.Transform.rotation = Quaternion.Lerp(_npc.Transform.rotation, Quaternion.LookRotation(_npc.Target.position - _npc.Transform.position), Time.deltaTime);
        }

        else if (_npc.Agent.velocity.magnitude > 0.1f)
        {
            _npc.Transform.rotation = Quaternion.Lerp(_npc.Transform.rotation, Quaternion.LookRotation(_npc.Agent.velocity.normalized), Time.deltaTime);
        }
    }
}
