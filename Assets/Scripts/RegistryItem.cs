using UnityEngine;
using UnityEngine.Serialization;

public class RegistryItem : ScriptableObject
{
    [SerializeField] 
    [FormerlySerializedAs("abilityID")] 
    [FormerlySerializedAs("iD")] protected string id;
    public string ID => id;
}
