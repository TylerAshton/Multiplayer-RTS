using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class AbilityRegistryTest : NetworkBehaviour
{
    private TMP_Dropdown dropdown;
    private void Awake()
    {
        if (!TryGetComponent<TMP_Dropdown>(out dropdown))
        {
            Debug.LogError("TMP_Dropdown is required for AbiltyRegistryTest");
            return;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        IReadOnlyDictionary<string, Ability> abilities = AbilityRegistry.Abilities;

        PopulateDropdown(abilities);
    }

    private void PopulateDropdown(List<string> _abilityIDs)
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(_abilityIDs);
    }

    private void PopulateDropdown(IReadOnlyDictionary<string, Ability> _abilities)
    {
        List<string> abilityIDs = new List<string>(_abilities.Keys);
        PopulateDropdown(abilityIDs);
    }

    /// <summary>
    /// Initiates the RPC call to test the ability selected in the dropdown
    /// </summary>
    public void FireTestRPC()
    {
        AbilityTestRpc(dropdown.options[dropdown.value].text);
    }

    /// <summary>
    /// RPC which sends the ability ID to everyone except the sender to test the ability registry
    /// </summary>
    /// <param name="_abilityID"></param>
    [Rpc(SendTo.NotMe)]
    private void AbilityTestRpc(string _abilityID)
    {
        Ability ability = AbilityRegistry.GetAbility(_abilityID);

        Debug.Log(ability);
    }

    
}
