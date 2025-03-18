using NUnit.Framework;
using System;
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

    public void AreaSelection(Rect _rect)
    {
        ClearAllSelectedUnits();

        foreach (Unit _unit in allUnits)
        {
            Vector3 unitScreenPos = Camera.main.WorldToScreenPoint(_unit.transform.position);
            Debug.Log($"{_rect} - {unitScreenPos}");
            if (_rect.Contains(unitScreenPos, true))
            {
                SelectUnit(_unit);
            }
        }
    }

    private void SelectUnit(Unit _unit)
    {
        selectedUnits.Add(_unit);
        _unit.ShowSelectionIndicator();
    }

    private void DeselectUnit(Unit _unit)
    {
        if (!selectedUnits.Contains(_unit))
        {
            Debug.LogError("Attempted to deselect a unit that isn't selected");
            return;
        }

        selectedUnits.Remove(_unit);
        _unit.HideSelectionIndicator();
    }

    private void ClearAllSelectedUnits()
    {
        List<Unit> cacheSelectedUnits = new List<Unit>();
        
        foreach (Unit _unit in selectedUnits)
        {
            cacheSelectedUnits.Add(_unit);
        }

        foreach(Unit _unit in cacheSelectedUnits)
        {
            DeselectUnit(_unit);
        }
    }

    /// <summary>
    /// Sets all selected units to move to a target position
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void MoveOrder(Vector3 _worldPosition)
    {
        foreach (Unit _unit in selectedUnits)
        {
            if (_unit is NPC _NPC)
            {
                _NPC.SetDestination(_worldPosition);
            }
        }
    }
}
