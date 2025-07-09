using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

class KnightShop : Shop
{
    private float angle;
    private float sectionAngle = 0f; // How much of the radial screen an option takes up. For example 2 options would give 180 each, 4 would give 90 each
    [SerializeField] private int healthCost = 500;
    protected override void Awake()
    {
        base.Awake();
        ID = NetworkManager.Singleton.LocalClientId;
    }

    private void Start()
    {
        sectionAngle = 360f / options.Length;

        // Dynamically set up the names
        for (int i = 1; i < options.Length; i++)
        {
            options[i].text = abilities[i-1].name;
        }
    }

    [Rpc(SendTo.Server)]
    private void PurchaseOption1Rpc(ulong _clientID)
    {
        this.GetComponentInParent<Health>().Heal(999);
        PointManager.Instance.RemovePoints(_clientID, 500);
    }

    [Rpc(SendTo.Server)]
    private void PurchaseOption2Rpc(ulong _clientID, int _selectedOptionIndex) // TODO: This is botched af
    {
        Ability selectedAbility = null;

        foreach (Ability _ability in abilities)
        {
            if (_ability.AbilityName == options[_selectedOptionIndex].text)
            {
                selectedAbility = _ability;
                break;
            }
        }

        if (selectedAbility == null)
        {
            Debug.LogError($"Ability missmatch: {options[_selectedOptionIndex].text}");
        }

        // TODO: Add return statement if selectedAbility == null

        // THIS CODE WAS ALSO WORKED ON BY HARRISON ON MY ACCOUNT, THIS IS HERE TO GIVE CREDIT DESPITE WHAT GITBLAME MIGHT REFLECT. SIGNED : TALINNETT

        this.GetComponentInParent<ChampionAbilityManager>().AddAbility(selectedAbility, 0);
        PointManager.Instance.RemovePoints(_clientID, 3000);
        
    }

    private void Update()
    {
        moveInput.x = Input.mousePosition.x - (Screen.width / 2f);
        moveInput.y = Input.mousePosition.y - (Screen.height / 2f);
        moveInput.Normalize();
        if (moveInput != Vector2.zero)
        {
            angle = Mathf.Atan2(moveInput.y, -moveInput.x) / Mathf.PI;
            angle = angle * 180;
            angle += 90f; //Rotate it so 0 degrees is at the bottom
            if (angle < 0)
            {
                angle += 360;
            }

            // Selection logic based on the angle of the cursor
            for (int i = 0; i < options.Length; i++)
            {
                if (angle > i * sectionAngle && angle < (i + 1) * sectionAngle)
                {
                    options[i].color = highlightedColour;
                    selectedOption = i;
                }
                else
                {
                    options[i].color = normalColour;
                }
            }
        }

        SelectOption();
        
    }

    /// <summary>
    /// Highlights the selected option and displays the cost, if the player clicks their mouse it will attempt to purchase the option
    /// </summary>
    private void SelectOption()
    {
        switch (selectedOption)
        {
            case 0:
                itemCostText.text = $"-{healthCost}";
                if (PointManager.Instance.GetPoints(ID) >= healthCost)
                {
                    itemCostText.color = Color.green;
                    if (Input.GetMouseButtonDown(0))
                    {
                        //PurchaseOption1Rpc(ID);
                        TryHealPlayerRpc(ID);
                        this.GetComponentInParent<AnimatedChampion>().ToggleUI();
                    }
                }
                else
                {
                    itemCostText.color = Color.red;
                }
                break;

            default:
                //itemCostText.text = "-3000";
                int price = abilities[selectedOption - 1].PurchasePrice;
                itemCostText.text = $"-{price}";
                if (PointManager.Instance.GetPoints(ID) >= price)
                {
                    itemCostText.color = Color.green;
                    if (!this.GetComponentInParent<ChampionAbilityManager>().CheckAbility(abilities[selectedOption - 1]))
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            //PurchaseOption2Rpc(ID, selectedOption);
                            TryPurchaseAbilityOptionRpc(selectedOption, ID);
                            this.GetComponentInParent<AnimatedChampion>().ToggleUI();
                        }
                    }
                }
                else
                {
                    itemCostText.color = Color.red;
                }
                break;
        }

    }

    [Rpc(SendTo.Server)]
    private void TryPurchaseAbilityOptionRpc(int _selectedOption, ulong _clientID)
    {
        if (_selectedOption < 0 || _selectedOption >= options.Length)
        {
            Debug.LogError($"Attempted to purchase an ability out of the bounds of {nameof(options)}: {_selectedOption}");
            return;
        }


        // Heal specific option
        if (_selectedOption == 0)
        {
            Debug.LogError($"Attempted to use purchase healing with {nameof(TryPurchaseAbilityOptionRpc)}!");
            return;
        }

        // Ability Purchasing
        Ability ability = abilities[_selectedOption - 1];
        
        if (ability == null)
        {
            Debug.LogError($"Ability of ID {selectedOption - 1} is null in {nameof(abilities)} in gameobject: {gameObject.name}!");
            return;
        }
        
        // Check if they already have the ability
        if (this.GetComponentInParent<ChampionAbilityManager>().CheckAbility(ability))
        {
            Debug.LogError($"Player: {_clientID} attempted to pruchase {ability.name} which they already have!");
            return;
        }

        if (PointManager.Instance.GetPoints(_clientID) < ability.PurchasePrice)
        {
            Debug.LogError($"Player: {_clientID} attempted to pruchase {ability.name} with insuficient funds {PointManager.Instance.GetPoints(_clientID)} < {ability.PurchasePrice}!");
            return;
        }

        this.GetComponentInParent<ChampionAbilityManager>().AddAbility(ability, 0);
        PointManager.Instance.RemovePoints(_clientID, ability.PurchasePrice);
    }

    [Rpc(SendTo.Server)]
    private void TryHealPlayerRpc(ulong _clientID)
    {
        if (PointManager.Instance.GetPoints(_clientID) < healthCost)
        {
            Debug.LogError($"Player: {_clientID} attempted to heal with insuficient funds {PointManager.Instance.GetPoints(_clientID)} < {healthCost}!");
            return;
        }

        this.GetComponentInParent<Health>().Heal(999);
        PointManager.Instance.RemovePoints(_clientID, healthCost);

        return;
    }
}


