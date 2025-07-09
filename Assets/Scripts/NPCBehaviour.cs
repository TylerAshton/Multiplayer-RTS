using UnityEngine;

public class NPCBehaviour : UnitBehaviour
{
    protected NPC npc;
    public override void Init()
    {
        base.Init();


        if (!TryGetComponent<Unit>(out unit))
        {
            //Debug.LogError($"{nameof(Unit)} is required for {GetType().Name} on gameobject: {gameObject.name}") We'd get error anyway
            return;
        }

        if (!(unit is NPC _npc))
        {
            Debug.LogError($"{nameof(Unit)} is not an {nameof(NPC)}. Cannot initialize {GetType().Name} on gameobject: {gameObject.name}.");
            return;
        }

        npc = _npc;
    }

    public override void Tick()
    {
        
    }
}
