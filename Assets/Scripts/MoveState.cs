using UnityEngine;

public class MoveState : State
{
    protected Vector3 destination;
    protected NPC npc;
    private readonly float waypointLeniance = 0.1f;
    public MoveState(Vector3 _destination, Unit _unit) : base(_unit)
    {
        destination = _destination;

        if (_unit is not NPC)
        {
            Debug.LogError("Attempted to start a moveState when the unit isn't an NPC");
        }
        npc = _unit as NPC;
        stateDebugColor = Color.blue;
    }

    public override void Enter()
    {
        npc.Agent.SetDestination(destination);
    }

    public override void Update()
    {
        if (IsComplete())
        {
            OnComplete();
        }
    }
    public override void Exit()
    {
        npc.Agent.ResetPath();
    }

    protected override bool IsComplete()
    {
        float distance = Vector3.Distance(unit.GetFeet(), destination);
        Debug.Log($"{distance}, {unit.GetFeet()}, {destination}");

        if (distance < waypointLeniance)
        {
            return true;
        }
        return false;
    }

    protected override void OnComplete()
    {
        IdleState idleState = new IdleState(unit);
        unit.ChangeState(idleState);
    }


}
