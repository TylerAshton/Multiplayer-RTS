using UnityEngine;

public class MoveTask : NPCTask
{
    protected Vector3 destination;
    private readonly float waypointLeniance = 0.1f;

    public MoveTask(NPC _npc, Vector3 _destination) : base(_npc)
    {
        destination = _destination;
    }
    public override void Start()
    {
        npc.Agent.SetDestination(destination);
    }

    public override void Exit()
    {
        npc.Agent.ResetPath();
    }

    public override void Update()
    {

    }

    public override bool IsComplete()
    {
        float distance = Vector3.Distance(unit.GetFeet(), destination);
        Debug.Log($"{distance}, {unit.GetFeet()}, {destination}");

        if (distance < waypointLeniance)
        {
            return true;
        }
        return false;
    }
}
