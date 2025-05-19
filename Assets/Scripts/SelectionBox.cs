using System;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SelectionBox : MonoBehaviour
{
    private RectTransform selectionRect;
    public RectTransform SelectionRect => selectionRect;
    private Vector2 startPos;


    public void Init()
    {
        selectionRect = GetComponent<RectTransform>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {

    }

    public void EnableBox()
    {
        gameObject.SetActive(true);
    }

    public void DisableBox()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Draws the selection box with the two corners given
    /// </summary>
    /// <param name="_startPos"></param>
    /// <param name="_endPos"></param>
    public void DrawSelectionBox(Vector2 _startPos, Vector2 _endPos)
    {
        // Calculate min and max
        float width = _endPos.x - _startPos.x;
        float height = _endPos.y - _startPos.y;

        selectionRect.anchoredPosition = _startPos;
        selectionRect.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));

        // Adjust pivot to keep selection box in correct direction
        selectionRect.pivot = new Vector2(width < 0 ? 1 : 0, height < 0 ? 1 : 0);
    }

    /// <summary>
    /// Returns the screenRect of the selectionBox
    /// </summary>
    /// <returns></returns>
    public Rect GetScreenRect()
    {
        Vector2 size = selectionRect.sizeDelta;
        Vector2 anchorPos = selectionRect.anchoredPosition;

        // Account for pivot to flip selection area correctly
        size.x = selectionRect.pivot.x == 0 ? size.x : -size.x;
        size.y = selectionRect.pivot.y == 0 ? size.y : -size.y;

        return new Rect(anchorPos, size);
    }
}
