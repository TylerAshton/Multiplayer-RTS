using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WinManager : NetworkBehaviour
{
    private int remainingAmalgams = 0;
    private int remainingChampions = 0;

    [SerializeField] private List<Health> StartingAmalgams = new List<Health>();

    private void Start()
    {
        foreach (var amalgam in StartingAmalgams) // Amalgams are set in the inspector
        {
            if (amalgam != null)
            {
                SelectAmalgam(amalgam);
            }
        }
    }

    public void SelectAmalgam(Health _amalgamHealth)
    {
        if (_amalgamHealth == null)
        {
            Debug.LogError("Amalgam Health cannot be null!");
            return;
        }

        remainingAmalgams++;
        _amalgamHealth.OnDeath += DeductRemainingAmalgams;
    }

    public void SelectChampion(Health _championHealth)
    {
        if (_championHealth == null)
        {
            Debug.LogError("Champion Health cannot be null!");
            return;
        }

        remainingChampions++;
        _championHealth.OnDeath += DeductRemainingChampions;
        _championHealth.OnRevive += IncreaseReminaingChampions;
    }

    public void DeductRemainingAmalgams()
    {
        remainingAmalgams--;

        if (remainingAmalgams <= 0)
        {
            DeclareChampionVictoryRpc();
        }
    }

    public void DeductRemainingChampions()
    {
        remainingChampions--;

        if (remainingChampions <= 0)
        {
            DeclareAmalgamVictoryRpc();
        }
    }

    public void IncreaseReminaingChampions()
    {
        remainingChampions++;
    }

    [Rpc(SendTo.Everyone)]
    private void DeclareAmalgamVictoryRpc()
    {
        Debug.Log("Amalgam victory declared!");
        // Additional logic for Amalgam victory can be added here
    }

    [Rpc(SendTo.Everyone)]
    private void DeclareChampionVictoryRpc()
    {
        Debug.Log("Champion victory declared!");
        // Additional logic for Champion victory can be added here
    }
}
