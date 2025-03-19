using UnityEngine;

public class IdleState : State
{
    public IdleState(Unit _unit) : base(_unit)
    {
    }

    public override void Enter()
    {
        Debug.Log("Entering Idle");
    }
    public override void Update()
    {
        
    }

    public override void Exit()
    {
        Debug.Log("Exitting Idle");
    }

}
