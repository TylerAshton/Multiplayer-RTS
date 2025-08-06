using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionItem : MonoBehaviour
{
    private SelectableObject selectableObject;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI quantity;
    public void Init(SelectableObject _selectableObject, int _quantity)
    {
        selectableObject = _selectableObject;
        image.sprite = selectableObject.SelectionIcon;
        quantity.text = _quantity.ToString();
    }
}
