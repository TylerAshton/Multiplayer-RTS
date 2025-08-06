using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectionUIManager : MonoBehaviour
{
    [SerializeField] private GameObject selectionGrid;
    [SerializeField] private GameObject selectionItemPrefab;

    private void Awake()
    {
        if (selectionGrid == null)
        {
            Debug.LogError($"{nameof(selectionGrid)} is not assigned in {GetType().Name}.");
            return;
        }
    }

    public void UpdateSelection(List<SelectableObject> _selectableObjects)
    {
        ClearSelectionGrid();

        Dictionary<SelectableObject, int> selectionCounts = 
        _selectableObjects.GroupBy(obj => obj.ID).ToDictionary(group => group.First(), group => group.Count());

        foreach (var kvp in selectionCounts)
        {
            CreateSelectionItem(kvp.Key, kvp.Value);
        }
    }

    private void CreateSelectionItem(SelectableObject _selectableObject, int _count)
    {
        GameObject selectionItemGO = Instantiate(selectionItemPrefab, selectionGrid.transform);
        if (!selectionItemGO.TryGetComponent<SelectionItem>(out SelectionItem selectionItem))
        {
            Debug.LogError($"{nameof(SelectionItem)} component is missing on the prefab: {selectionItemPrefab.name}");
            Destroy(selectionItemGO);
            return;
        }
        selectionItem.Init(_selectableObject, _count);
    }

    public void ClearSelectionGrid()
    {
        foreach (Transform child in selectionGrid.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
