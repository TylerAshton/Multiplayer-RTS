using UnityEngine;

/// <summary>
/// Reusabled behaviour for NPCs such as melee and ranged NPCs.
/// </summary>
public abstract class NPCBehaviour : ScriptableObject
{
    /// <summary>
    /// Called when the NPC is initialized.
    /// </summary>
    /// <param name="npc"></param>
    public abstract void Init(NPC _npc);

    /// <summary>
    /// Called on update for the NPC.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="deltaTime"></param>
    public abstract void Update(NPC _npc, float _deltaTime);

    /// <summary>
    /// Called when the NPC selects a target.
    /// </summary>
    /// <param name="npc"></param>
    public abstract void OnSetTarget(NPC _npc);

    /// <summary>
    /// Called when the NPC clears its target.
    /// </summary>
    /// <param name="npc"></param>
    public abstract void OnClearTarget(NPC _npc);
}
