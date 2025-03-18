using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NPCs are mobile units which use the nav mesh.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class NPC : Unit
{
    private NavMeshAgent agent;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// Sets the nav agent's destination to the give position
    /// </summary>
    /// <param name="_worldPosition"></param>
    public void SetDestination(Vector3 _worldPosition)
    {
        agent.SetDestination(_worldPosition);
    }
}
