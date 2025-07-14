using UnityEngine;
using UnityEngine.Serialization;

public class RegistryItem : ScriptableObject
{
    [SerializeField] [FormerlySerializedAs("abilityID")] protected string id;
    public string ID => id;
}
