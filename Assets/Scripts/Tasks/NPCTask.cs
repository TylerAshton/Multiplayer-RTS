using UnityEngine;

public class NPCTask : Task
{
    protected NPC npc;
    public NPCTask(NPC _npc) : base(_npc)
    {
        this.npc = _npc;
    }

    public override void Start()
    {
        throw new System.NotImplementedException();
    }

    public override void Exit()
    {
        throw new System.NotImplementedException();
    }

    public override void Update()
    {
        throw new System.NotImplementedException();
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
