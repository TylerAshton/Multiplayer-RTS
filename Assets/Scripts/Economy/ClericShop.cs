using Unity.Netcode;
using UnityEngine;

class ClericShop : Shop
{
    protected override void Awake()
    {
        base.Awake();
        ID = NetworkManager.Singleton.LocalClientId;
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

        this.GetComponentInParent<ChampionAbilityManager>().AddAbility(selectedAbility);
        PointManager.Instance.RemovePoints(_clientID, 3000);
    }

    private void Update()
    {
        moveInput.x = Input.mousePosition.x - (Screen.width / 2f);
        moveInput.y = Input.mousePosition.y - (Screen.height / 2f);
        moveInput.Normalize();
        if (moveInput != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveInput.y, -moveInput.x) / Mathf.PI;
            angle = angle * 180;
            angle += 90f; //Rotate it so 0 degrees is at the bottom
            if (angle < 0)
            {
                angle += 360;
            }

            for (int i = 0; i < options.Length; i++)
            {
                if (angle > i * 180 && angle < (i + 1) * 180)
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

        switch (selectedOption)
        {
            case 0:
                itemCostText.text = "-500";
                if (PointManager.Instance.GetPoints(ID) >= 500)
                {
                    itemCostText.color = Color.green;
                    if (Input.GetMouseButtonDown(0))
                    {
                        PurchaseOption1Rpc(ID);
                        this.GetComponentInParent<AnimatedChampion>().ToggleUI();
                    }
                }
                else
                {
                    itemCostText.color = Color.red;
                }
                break;
            case 1:
                itemCostText.text = "-3000";
                if (PointManager.Instance.GetPoints(ID) >= 3000)
                {
                    itemCostText.color = Color.green;
                    if (!this.GetComponentInParent<ChampionAbilityManager>().CheckAbility(abilities[selectedOption - 1]))
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            PurchaseOption2Rpc(ID, selectedOption);
                            this.GetComponentInParent<AnimatedChampion>().ToggleUI();
                        }
                    }
                }
                else
                {
                    itemCostText.color = Color.red;
                }
                break;
            default:
                itemCostText.text = string.Empty;
                break;
        }
    }
}

