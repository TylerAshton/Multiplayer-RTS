using UnityEditor;
using UnityEngine;

public abstract class BaseAbilityStat : RegistryItem, Inspectorable
{
    public virtual bool IsValid()
    {
        if (this.ID == null || this.ID.Trim().Length == 0) // Use this instead of string.IsNullOrEmpty as it also checks for whitespace
        {
            Debug.LogError($"{this.name} has no ID assigned or ID is empty.");
            return false;
        }
        return true;
    }
}
