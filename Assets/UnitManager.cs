using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [SerializeField] private List<Unit> allUnits = new List<Unit>();
    [SerializeField] private List<Unit> selectedUnits = new List<Unit>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Adds unit to units list. Will not allow duplicate units to be added
    /// </summary>
    /// <param name="_unit"></param>
    public void AddUnit(Unit _unit)
    {
        if (allUnits.Contains(_unit))
        {
            Debug.LogError("AddUnit was called when the unit already exists in the list");
            return;
        }
        allUnits.Add(_unit);
    }

    public void RemoveUnit(Unit _unit)
    {
        allUnits.Remove(_unit);
    }
}
