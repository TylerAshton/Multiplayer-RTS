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

    public void AddUnit(Unit unit)
    {
        if (allUnits.Contains(unit))
        {
            Debug.LogError("AddUnit was called when the unit already exists in the list");
            return;
        }
        allUnits.Add(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        allUnits.Remove(unit);
    }
}
