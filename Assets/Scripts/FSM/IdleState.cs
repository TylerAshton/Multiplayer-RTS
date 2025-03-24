using UnityEngine;

public class IdleState : State
{
    protected NPC npc;
    public IdleState(Unit _unit) : base(_unit)
    {
        if (_unit is not NPC)
        {
            Debug.LogError("Attempted to start a moveState when the unit isn't an NPC");
        }
        npc = _unit as NPC;
        stateDebugColor = Color.white;
    }

    public override void Enter()
    {
        //Debug.Log("Entering Idle");
    }
    public override void Update()
    {
        npc.ScanForTarget();
    }

    public override void Exit()
    {
        //Debug.Log("Exitting Idle");
    }

    protected override bool IsComplete()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnComplete()
    {
        throw new System.NotImplementedException();
    }
}
