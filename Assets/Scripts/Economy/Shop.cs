using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Shop : NetworkBehaviour
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

    }
}

