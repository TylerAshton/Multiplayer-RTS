using System.Collections.Generic;
using UnityEngine;

public static class Registry<T> where T : RegistryItem
{
    private static Dictionary<string, T> items = new Dictionary<string, T>();

    public static IReadOnlyDictionary<string, T> Items => items; 

    /// <summary>
    /// Adds the registryItem into the items dictionary
    /// </summary>
    /// <param name="_newItem"></param>
    public static void Register(T _newItem)
    {
        string itemID = _newItem.ID;
        if (items.ContainsKey(itemID))
        {
            Debug.LogError($"Attempted to register an registryItem (ID: {itemID}, FileName: {_newItem.name}) that is alrady registered");
            return;
        }
        if (itemID == null || itemID == string.Empty)
        {
            Debug.LogError($"Registry ID of {_newItem.name} | {itemID} is null or empty.");
            return;
        }

        items.Add(itemID, _newItem);
    }

    /// <summary>
    /// Gets the item from the Abilities dictionary with the parsed ID
    /// </summary>
    /// <param name="_itemID"></param>
    /// <returns></returns>
    public static T GetItem(string _itemID)
    {
        T output = items.TryGetValue(_itemID, out T item) ? item : null;

        if (output == null)
        {
            Debug.LogError($"Item ID {_itemID} does not exist in the registry.");
        }

        return output;
    }

    /*[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]*/
    public static void Init(string _resourcePath = "")
    {
        Registry<T>.items.Clear();

        var items = Resources.LoadAll<T>(_resourcePath);
        foreach (var item in items)
        {
            Register(item);
        }
    }

}
