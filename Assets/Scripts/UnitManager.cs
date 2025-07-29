using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;

public class UnitManager : NetworkBehaviour
{
    [SerializeField] private List<SelectableObject> allUnits = new List<SelectableObject>();
    [SerializeField] private List<SelectableObject> selectedUnits = new List<SelectableObject>();
    [SerializeField] private GameObject AbilityPanelPrefab;
    [SerializeField] private GameObject ConstructionPanelPrefab;
    [SerializeField] private LayerMask unitLayer;
    private AbilityUIManager abilityUIManager;
    private ConstructionUIManager constructionUIManager;
    private RTSPlayerControls rTSPlayerControls;
    private bool isShiftHeld => rTSPlayerControls.IsShiftPressed;
    public List<SelectableObject> SelectedUnits => new List<SelectableObject>(selectedUnits);

    private readonly float moveSpacing = 2;
    private readonly int moveLayerCapciaty = 8;


    private void Awake()
    {
        if (!TryGetComponent<RTSPlayerControls>(out rTSPlayerControls))
        {
            Debug.LogError($"{nameof(rTSPlayerControls)} is required in {GetType().Name} within gameobject {gameObject.name}!");
            return;
        }
    }

    public void Init()
    {
        GameObject AbilityPanel = Instantiate(AbilityPanelPrefab);
        abilityUIManager = AbilityPanel.GetComponentInChildren<AbilityUIManager>();

        GameObject ConstructionPanel = Instantiate(ConstructionPanelPrefab);
        constructionUIManager = ConstructionPanel.GetComponentInChildren<ConstructionUIManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Adds unit to units list. Will not allow duplicate units to be added
    /// </summary>
    /// <param name="_unit"></param>
    public void AddUnit(SelectableObject _unit)
    {
        if (_unit == null)
        {
            Debug.LogError("Attempted to add a null unit");
            return;
        }

        if (allUnits.Contains(_unit))
        {
            Debug.LogError("AddUnit was called when the unit already exists in the list");
            return;
        }
        allUnits.Add(_unit);
    }

    /// <summary>
    /// Removes the unit from the allUnits and selectedUnits lists.
    /// </summary>
    /// <param name="_unit"></param>
    public void RemoveUnit(SelectableObject _unit)
    {
        if (_unit == null)
        {
            Debug.LogError("Attempted to remove a null unit");
            return;
        }

        allUnits.Remove(_unit);

        if (selectedUnits.Contains(_unit))
        {
            DeselectUnit(_unit); // Deselect the unit if it's selected
        }
    }

    /// <summary>
    /// Selects all units inside the (screenSpace) rect
    /// </summary>
    /// <param name="_rect"></param>
    public void AreaSelection(Rect _rect)
    {
        if (_rect == null)
        {
            Debug.LogError("Attempted to select units with a null rect");
            return;
        }

        if (!isShiftHeld)
        {
            ClearAllSelectedUnits();
        }

        foreach (SelectableObject _unit in allUnits)
        {
            if (!_unit.IsSelectable)
            {
                continue;
            }
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
    public void SelectUnit(SelectableObject _unit)
    {
        if (_unit == null)
        {
            Debug.LogError("Attempted to select a null unit");
            return;
        }

        if (selectedUnits.Contains(_unit))
        {
            Debug.LogError("Attempted to select a unit that is already selected");
            return;
        }

        selectedUnits.Add(_unit);
        abilityUIManager.UpdateAbilityTabsWithUnitSelection(selectedUnits); // TODO: This is a bit inefficeint
        constructionUIManager.UpdateUI(selectedUnits);
        _unit.SelectionHighlighter.SetSelectionMode(SelectionMode.Select);
    }

    /// <summary>
    /// Removes the unit from the selectedUnits list and hides its selection indicator
    /// </summary>
    /// <param name="_unit"></param>
    public void DeselectUnit(SelectableObject _unit)
    {
        if (_unit == null)
        {
            Debug.LogError("Attempted to deselect a null unit");
            return;
        }

        if (!selectedUnits.Contains(_unit))
        {
            Debug.LogError("Attempted to deselect a unit that isn't selected");
            return;
        }

        selectedUnits.Remove(_unit);

        if (selectedUnits.Count > 0)
        {
            abilityUIManager.UpdateAbilityTabsWithUnitSelection(selectedUnits); // TODO: This is a bit inefficeint
            constructionUIManager.UpdateUI(selectedUnits);
        }
        else
        {
            abilityUIManager.ResetAbilityGrid();
            constructionUIManager.ResetManager();
        }
        _unit.SelectionHighlighter.SetSelectionMode(SelectionMode.None);
    }

    /// <summary>
    /// Botched. Tries to deselect a unit if it's selected
    /// </summary>
    /// <param name="constructionPad"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void TryDeselectUnit(SelectableObject _unit)
    {
        if (_unit == null)
        {
            Debug.LogError("Attempted to deselect a null unit");
            return;
        }

        if (selectedUnits.Contains(_unit))
        {
            DeselectUnit(_unit);
        }
    }

    /// <summary>
    /// Runs the DeselectUnit function on all units that are selected
    /// </summary>
    private void ClearAllSelectedUnits()
    {
        List<SelectableObject> cacheSelectedUnits = new List<SelectableObject>();
        
        foreach (SelectableObject _unit in selectedUnits)
        {
            cacheSelectedUnits.Add(_unit);
        }

        foreach(SelectableObject _unit in cacheSelectedUnits)
        {
            DeselectUnit(_unit); // TODO: This is a bit inefficient as it refreshes several times
        }

        abilityUIManager.ClearUI();
        constructionUIManager.ResetManager();
    }

    /// <summary>
    /// Sets all selected units to move to a target position
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void MoveOrder(Vector3 _worldPosition)
    {
        if (_worldPosition == null)
        {
            Debug.LogError("Attempted to move units to a null position");
            return;
        }

        for (int i = 0; i < selectedUnits.Count; i++)
        {
            if (selectedUnits[i] is NPC _NPC)
            {
                Vector3 targetPosition = _worldPosition + CalculateFormationOffset(i);
                MoveTask moveTask = new MoveTask(_NPC, GetNavPos(targetPosition));
                _NPC.ImposeNewTask(moveTask);
            }
        }
    }

    /// <summary>
    /// Returns a valid NavMesh position for the given world position.
    /// </summary>
    /// <param name="_worldPosition"></param>
    /// <returns></returns>
    private Vector3 GetNavPos(Vector3 _worldPosition)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(_worldPosition, out hit, 1.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            Debug.LogError($"Failed to find a valid NavMesh position for {_worldPosition}");
            return _worldPosition; // I'd return zero but that'd be even worse
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

    /// <summary>
    /// Raycasts to the position selecting the first unit hit
    /// </summary>
    /// <param name="_mouseScreenPos"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void PointSelection(Vector2 _mouseScreenPos)
    {
        if (!isShiftHeld)
        {
            ClearAllSelectedUnits();
        }

        SelectableObject clickedUnit = GetSelectableAtMouse(_mouseScreenPos);

        if (clickedUnit == null)
        {
            return;
        }

        if (selectedUnits.Contains(clickedUnit))
        {
            return;
        }

        SelectUnit(clickedUnit);
    }

    private SelectableObject GetSelectableAtMouse(Vector3 _mouseScreenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(_mouseScreenPos);

        SelectableObject clickedUnit = null;

        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, unitLayer))
        {
            GameObject hitObject = hitInfo.collider.gameObject;

            // Find matching unit in cache by GameObject reference
            clickedUnit = allUnits.Find(unit => unit.gameObject == hitObject);

            if (clickedUnit == null)
            {
                Debug.LogError($"{hitInfo.collider.gameObject.name} was not found in {allUnits}!");
                return null;
            }

            if (!clickedUnit.IsSelectable)
            {
                return null;
            }
        }

        

        return clickedUnit;
    }

    public void SelectCommon(Vector2 _mouseScreenPos)
    {
        SelectableObject clickedUnit = GetSelectableAtMouse(_mouseScreenPos);

        if (clickedUnit == null)
        {
            return;
        }

        SelectableObject[] matchingUnits;

        if (!isShiftHeld) // If double clicking we'll already have selected the clickUnit
        {
            ClearAllSelectedUnits();
            matchingUnits = allUnits.Where(unit => unit.ID == clickedUnit.ID && unit.IsSelectable).ToArray();
        }
        else
        {
            matchingUnits = allUnits.Where(unit => unit.ID == clickedUnit.ID && unit.IsSelectable && !selectedUnits.Contains(unit)).ToArray();
        }

        foreach (SelectableObject _selectableObject in matchingUnits)
        {
            SelectUnit(_selectableObject);
        }
    }
}
