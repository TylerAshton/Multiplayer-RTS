using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class UnitManager : NetworkBehaviour
{
    [SerializeField] private List<Unit> allUnits = new List<Unit>();
    [SerializeField] private List<Unit> selectedUnits = new List<Unit>();
    [SerializeField] private GameObject AbilityPanelPrefab;
    private AbilityUIManager unitControlsManager;
    public List<Unit> SelectedUnits => new List<Unit>(selectedUnits);

    private readonly float moveSpacing = 2;
    private readonly int moveLayerCapciaty = 8;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject AbilityPanel = Instantiate(AbilityPanelPrefab);
        unitControlsManager = AbilityPanel.GetComponentInChildren<AbilityUIManager>();
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

    /// <summary>
    /// Selects all units inside the (screenSpace) rect
    /// </summary>
    /// <param name="_rect"></param>
    public void AreaSelection(Rect _rect)
    {
        ClearAllSelectedUnits();

        foreach (Unit _unit in allUnits)
        {
            Vector3 unitScreenPos = Camera.main.WorldToScreenPoint(_unit.transform.position);
            //Debug.Log($"{_rect} - {unitScreenPos}");
            if (_rect.Contains(unitScreenPos, true))
            {
                SelectUnit(_unit);
            }
        }
    }

    /// <summary>
    /// Adds the unit to the selectedUNits list and shows its selection indicator
    /// </summary>
    /// <param name="_unit"></param>
    private void SelectUnit(Unit _unit)
    {
        selectedUnits.Add(_unit);
        unitControlsManager.UpdateGridWithUnitSelection(selectedUnits); // TODO: This is a bit inefficeint
        _unit.ShowSelectionIndicator();
    }

    /// <summary>
    /// Removes the unit from the selectedUnits list and hides its selection indicator
    /// </summary>
    /// <param name="_unit"></param>
    private void DeselectUnit(Unit _unit)
    {
        if (!selectedUnits.Contains(_unit))
        {
            Debug.LogError("Attempted to deselect a unit that isn't selected");
            return;
        }

        selectedUnits.Remove(_unit);

        if (selectedUnits.Count > 0)
        {
            unitControlsManager.UpdateGridWithUnitSelection(selectedUnits); // TODO: This is a bit inefficeint
        }
        else
        {
            unitControlsManager.ResetAbilityGrid();
        }
        _unit.HideSelectionIndicator();
    }

    /// <summary>
    /// Runs the DeselectUnit function on all units that are selected
    /// </summary>
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
        for (int i = 0; i < selectedUnits.Count; i++)
        {
            if (selectedUnits[i] is NPC _NPC)
            {
                Vector3 targetPosition = _worldPosition + CalculateFormationOffset(i);
                MoveTask moveTask = new MoveTask(_NPC, targetPosition);
                _NPC.ImposeNewTask(moveTask);
            }
        }
    }

    /// <summary>
    /// Calculates the position offset for a unit in the formation
    /// </summary>
    private Vector3 CalculateFormationOffset(int index)
    {
        if (index == 0) return Vector3.zero;

        // Take away 1 from index calculations as the first unit is always ignored
        int layer = (index - 1) / moveLayerCapciaty + 1; // Each layer has moveLayerCapciaty (8?) units;

        int positionInLayer = (index - 1) % moveLayerCapciaty;

        float angle = positionInLayer * (360 / moveLayerCapciaty);

        // Calculate offset position in the circle
        float radius = layer * moveSpacing;
        float radian = Mathf.Deg2Rad * angle;

        Vector3 offset = new Vector3(Mathf.Cos(radian) * radius, 0, Mathf.Sin(radian) * radius);

        return offset;
    }
}
