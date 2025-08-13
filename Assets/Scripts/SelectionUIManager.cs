using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SelectionUIManager : MonoBehaviour
{
    [SerializeField] private GameObject selectionGrid;
    [SerializeField] private GameObject selectionItemPrefab;
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private TextMeshProUGUI detailPanelText;

    private void Awake()
    {
        if (selectionGrid == null)
        {
            Debug.LogError($"{nameof(selectionGrid)} is not assigned in {GetType().Name}.");
            return;
        }

        if (selectionItemPrefab == null)
        {
            Debug.LogError($"{nameof(selectionItemPrefab)} is not assigned in {GetType().Name}.");
            return;
        }

        if (detailPanel == null)
        {
            Debug.LogError($"{nameof(detailPanel)} is not assigned in {GetType().Name}.");
            return;
        }

        if (detailPanelText == null)
        {
            Debug.LogError($"{nameof(detailPanelText)} is not assigned in {GetType().Name}.");
            return;
        }
    }

    public void UpdateSelection(List<SelectableObject> _selectableObjects)
    {
        ClearSelectionUI();

        Dictionary<SelectableObject, int> selectionCounts = 
        _selectableObjects.GroupBy(obj => obj.ID).ToDictionary(group => group.First(), group => group.Count());

        if (selectionCounts.Count == 1)
        {
            KeyValuePair<SelectableObject, int> selectionItem = selectionCounts.First();
            CreateDetailedSelectionItem(selectionItem.Key, selectionItem.Value);

            return;
        }

        foreach (KeyValuePair<SelectableObject, int> selectionItem in selectionCounts)
        {
            CreateSelectionItem(selectionItem.Key, selectionItem.Value);
        }
    }

    private void CreateSelectionItem(SelectableObject _selectableObject, int _count)
    {
        if (_selectableObject == null)
        {
            Debug.LogError($"{nameof(_selectableObject)} cannot be null!");
            return;
        }

        if (_count < 1)
        {
            Debug.LogError("Can't have a non or negative count!");
            return;
        }

        GameObject selectionItemGO = Instantiate(selectionItemPrefab, selectionGrid.transform);

        if (!selectionItemGO.TryGetComponent<SelectionItem>(out SelectionItem selectionItem))
        {
            Debug.LogError($"{nameof(SelectionItem)} component is missing on the prefab: {selectionItemPrefab.name}");
            Destroy(selectionItemGO);
            return;
        }

        selectionItem.Init(_selectableObject, _count);
    }

    private void CreateDetailedSelectionItem(SelectableObject _selectableObject, int _count)
    {
        if (_selectableObject == null)
        {
            Debug.LogError($"{nameof(_selectableObject)} cannot be null!");
            return;
        }

        if (_count < 1)
        {
            Debug.LogError("Can't have a negative count!");
            return;
        }

        if (string.IsNullOrEmpty(_selectableObject.Description))
        {
            Debug.LogError($"{nameof(SelectableObject)} has no {nameof(_selectableObject.Description)}!");
            return;
        }

        CreateSelectionItem(_selectableObject, _count);

        detailPanel.SetActive(true);
        detailPanelText.text = _selectableObject.Description;
    }

    public void ClearSelectionUI()
    {
        ClearSelectionGrid();
        ClearSelectionDetails();
    }

    private void ClearSelectionGrid()
    {
        foreach (Transform child in selectionGrid.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void ClearSelectionDetails()
    {
        detailPanel.SetActive(false);
        detailPanelText.text = string.Empty;
    }


}
