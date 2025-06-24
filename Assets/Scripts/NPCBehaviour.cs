using UnityEngine;

public class NPCBehaviour : UnitBehaviour
{
    protected NPC npc;
    public override void Init(Unit _unit)
    {
        base.Init(_unit);

        if (_unit == null)
        {
            //Debug.LogError("_unit cannot be null"); We'd get error anyway
            return;
        }

        if (!(_unit is NPC _npc))
        {
            Debug.LogError("Unit is not an NPC. Cannot initialize NPCBehaviour.");
            return;
        }

        npc = _npc;
    }

    public override void Tick()
    {
        
    }
}
