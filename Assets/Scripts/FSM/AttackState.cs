using UnityEngine;

public class AttackState : State
{
    protected NPC npc;
    public AttackState(Unit _unit) : base(_unit)
    {
        if (_unit is not NPC)
        {
            Debug.LogError("Attempted to start a moveState when the unit isn't an NPC");
        }
        npc = _unit as NPC;
        stateDebugColor = Color.red;
    }

    public override void Enter()
    {
        npc.fireTime = 0;
        npc.TargetHealth.OnDeath += OnComplete;
    }

    public override void Exit()
    {
        npc.fireTime = 0;
    }

    public override void Update()
    {
        npc.fireTime += Time.deltaTime;

        if (npc.fireTime > npc.FireCoolDown)
        {
            npc.Shoot();
            npc.fireTime = 0;
        }
    }

    protected override bool IsComplete()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnComplete()
    {
        if (npc.TargetHealth != null) // TODO: This is a bit iffy as Target is being sudo managed by Attackstate and npc
        {
            npc.TargetHealth.OnDeath -= OnComplete;
        }
        IdleState idleState = new IdleState(unit);
        unit.ChangeState(idleState);
    }
}
