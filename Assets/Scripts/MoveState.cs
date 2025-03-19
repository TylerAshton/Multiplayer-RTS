using UnityEngine;

public class MoveState : State
{
    protected Vector3 destination;
    protected NPC npc;
    public MoveState(Vector3 _destination, Unit _unit) : base(_unit)
    {
        destination = _destination;

        if (_unit is not NPC)
        {
            Debug.LogError("Attempted to start a moveState when the unit isn't an NPC");
        }
        npc = _unit as NPC;
    }

    public override void Enter()
    {
        npc.Agent.SetDestination(destination);
    }


    public override void Update()
    {
        
    }
    public override void Exit()
    {
        npc.Agent.ResetPath();
    }
}
