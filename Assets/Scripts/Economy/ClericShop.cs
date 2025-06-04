using Unity.Netcode;
using UnityEngine;

class ClericShop : Shop
{
    protected override void Awake()
    {
        base.Awake();
        Debug.Log("I AM CLERIC");
        ID = NetworkManager.Singleton.LocalClientId;
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
                        this.GetComponentInParent<Health>().Heal(999);
                        PointManager.Instance.RemovePoints(ID, 500);
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
                            Ability selectedAbility = null;

                            foreach (Ability _ability in abilities)
                            {
                                if (_ability.AbilityName == options[selectedOption].text)
                                {
                                    selectedAbility = _ability;
                                    break;
                                }
                            }

                            // TODO: Add return statement if selectedAbility == null

                            // THIS CODE WAS ALSO WORKED ON BY HARRISON ON MY ACCOUNT, THIS IS HERE TO GIVE CREDIT DESPITE WHAT GITBLAME MIGHT REFLECT. SIGNED : TALINNETT

                            this.GetComponentInParent<ChampionAbilityManager>().AddAbility(selectedAbility);
                            PointManager.Instance.RemovePoints(ID, 3000);
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

