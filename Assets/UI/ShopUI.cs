using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopUI : MonoBehaviour
{
    private const int abilityCap = 4;

    private Button healthButton;
    private Button abilityButton1;
    private Button abilityButton2;
    private Button abilityButton3;
    private Button abilityButton4;

    private Label label;

    private List<Button> abilityButtons = new List<Button>();
    [SerializeField] private Ability[] purchasableAbilities = new Ability[abilityCap];


    private void Awake()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        label = root.Q<Label>("PriceLabel");
        healthButton = root.Q<Button>("Heal");
        abilityButton1 = root.Q<Button>("Ability1");
        abilityButton2 = root.Q<Button>("Ability2");
        abilityButton3 = root.Q<Button>("Ability3");
        abilityButton4 = root.Q<Button>("Ability4");

        abilityButtons.Add(abilityButton1);
        abilityButtons.Add(abilityButton2);
        abilityButtons.Add(abilityButton3);
        abilityButtons.Add(abilityButton4);

        if (purchasableAbilities.Length > abilityCap)
        {
            Debug.LogError($"{nameof(purchasableAbilities)} {purchasableAbilities.Length} exceeds ability cap of {abilityCap}!");
            return;
        }

        for (int i = 0; i < purchasableAbilities.Length && purchasableAbilities[i] != null; i++)
        {
            //Debug.Log(i);
        }
    }

    private void OnEnable()
    {
        ButtonActionsSubscribe();
    }

    private void OnDisable()
    {
        ButtonActionsUnsubscribe();
    }

    private void ButtonActionsUnsubscribe()
    {
        //healthButton.clicked -= HealthButton_clicked;
    }

    private void ButtonActionsSubscribe()
    {
        healthButton.RegisterCallback<PointerEnterEvent>(evt => CostDisplay(500));

        for (int i = 1; i < purchasableAbilities.Length; i++)
        {
            abilityButtons[i].RegisterCallback<PointerEnterEvent>(evt => CostDisplay(purchasableAbilities[i].PurchasePrice));
            Debug.Log(i);
        }
        /*
                healthButton.clicked += HealthButton_clicked;
                healthButton.RegisterCallback<PointerEnterEvent>(evt => OnHealthHover());*/

    }

    private void CostDisplay(int _cost)
    {
        label.text = $"Cost: {_cost}";
    }
}
