using System.Collections.Generic;
using UnityEditor.PackageManager.ValidationSuite;
using UnityEngine;

public class PurchaseRegistry
{
    private static Dictionary<string, Purchasable> purchasables = new Dictionary<string, Purchasable>();

    public static IReadOnlyDictionary<string, Purchasable> Purchasables => purchasables;

    /// <summary>
    /// Adds the ability into the Abilities dictionary
    /// </summary>
    /// <param name="_purchaseID"></param>
    /// <param name="_purchasable"></param>
    public static void Register(string _purchaseID, Purchasable _purchasable)
    {
        if (purchasables.ContainsKey(_purchaseID))
        {
            Debug.LogError($"Attempted to register an {_purchasable.GetType().Name} ({_purchaseID}) that is alrady registered");
            return;
        }
        if (_purchaseID == null || _purchaseID == string.Empty)
        {
            Debug.LogError($"{_purchasable.GetType().Name} ID | {_purchaseID} is null or empty.");
            return;
        }

        purchasables.Add(_purchaseID, _purchasable);
/*        Debug.Log($"Registered {_purchaseID}");*/
    }

    /// <summary>
    /// Gets the ability from the Abilities dictionary with the parsed ID
    /// </summary>
    /// <param name="_purchaseID"></param>
    /// <returns></returns>
    public static Purchasable GetPurchasable(string _purchaseID)
    {
        Purchasable output = purchasables.TryGetValue(_purchaseID, out Purchasable ability) ? ability : null;

        if (output == null)
        {
            Debug.LogError($"{nameof(Purchasable)} ID {_purchaseID} does not exist in the registry.");
        }

        return output;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoRegisterAll()
    {
        purchasables.Clear();

        var all = Resources.LoadAll<Purchasable>("");
        foreach (var a in all)
        {
            Register(a.PurchaseID, a);
        }
    }
}
