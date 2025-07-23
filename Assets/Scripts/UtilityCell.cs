using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UtilityCell : UIActionCell
{
    private List<AbilityManager> abilityManagers;
    private Action onClickAction;
    [SerializeField] private Color disabledColor = Color.gray;
    [SerializeField] private Color enabledColor = Color.white;

    public void Refresh(List<AbilityManager> _abilityManagers, Action _onClick)
    {
        abilityManagers = _abilityManagers;
        onClickAction = _onClick;

        image.enabled = true;
        button.interactable = true;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickAction.Invoke());
    }

    private void Update()
    {
        if (abilityManagers == null || abilityManagers.Count == 0)
        {
            image.color = disabledColor;
            return;
        }

        bool shouldGlow = abilityManagers.All(m => m.IsUtilityEnabled);

        image.color = shouldGlow ? enabledColor : disabledColor;
    }

    public void ResetCell()
    {
        Image.enabled = false;
        Button.interactable = false;
    }
}
