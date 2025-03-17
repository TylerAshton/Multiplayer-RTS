using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SelectionBox : MonoBehaviour
{
    private RectTransform selectionBox;
    private Vector2 startPos;
    private Rect selectionRect;


    private void Awake()
    {
        selectionBox = GetComponent<RectTransform>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DrawSelectionBox(Vector2 _startPos, Vector2 _endPos)
    {
        // Calculate min and max
        float width = _endPos.x - _startPos.x;
        float height = _endPos.y - _startPos.y;

        selectionBox.anchoredPosition = _startPos;
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));

        // Adjust pivot to keep selection box in correct direction
        selectionBox.pivot = new Vector2(width < 0 ? 1 : 0, height < 0 ? 1 : 0);
    }
}
