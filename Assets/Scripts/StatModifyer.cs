using UnityEngine;

[System.Serializable]
public class StatModifyer
{
    [SerializeField] private StatType statType;
    public StatType StatType => statType;

    [SerializeField] private float value;
    public float Value => value;    

    public StatModifyer() // Inspector friendly constructor
    {

    }

    public StatModifyer(StatType _statType, float _value) // Manual constructor for runtime.
    {
        this.statType = _statType;
        this.value = _value;
    }



}
