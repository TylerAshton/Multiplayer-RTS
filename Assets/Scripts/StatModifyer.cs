using UnityEngine;

[CreateAssetMenu(fileName = "New Stat Modifyer", menuName = "StatModifiers/New Modifier")]
public class StatModifyer : ScriptableObject
{
    [SerializeField] private StatType statType;
    public StatType StatType => StatType;

    [SerializeField] private float value;
    public float Value => value;    
}
