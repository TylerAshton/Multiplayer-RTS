using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public Vector2 moveInput;
    public TextMeshProUGUI[] options;
    [SerializeField] protected List<Ability> abilities;
    public Color normalColour, highlightedColour;

    protected int selectedOption;

    protected ulong ID;

    [SerializeField] protected TextMeshProUGUI points;
    [SerializeField] protected TextMeshProUGUI itemCostText;

    protected virtual void Awake()
    {
        Debug.Log("I AM A SHOP");
    }
}

