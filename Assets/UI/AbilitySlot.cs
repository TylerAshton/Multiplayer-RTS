using UnityEngine;
using UnityEngine.UIElements;

public class AbilitySlot : PurchaseSlot
{
    private Ability abilityData;
    protected override int Price => abilityData.PurchasePrice;

    public AbilitySlot(VisualElement _purchaseSlot, Ability _abilityData) : base(_purchaseSlot)
    {
        if (_abilityData == null)
        {
            Debug.LogError($"{nameof(_abilityData)} is null in {GetType().Name}!");
            return;
        }
        this.abilityData = _abilityData;
    }

}
