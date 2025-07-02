using UnityEngine;
public struct StatModifyer
{
    [SerializeField] private StatType statType;
    public StatType StatType => statType;

    [SerializeField] private float value;
    public float Value => value;    

    public StatModifyer(StatType _statType, float _value)
    {
        this.statType = _statType;
        this.value = _value;
    }



}
