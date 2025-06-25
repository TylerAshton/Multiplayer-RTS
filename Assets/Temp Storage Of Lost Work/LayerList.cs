using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LayerList : MonoBehaviour
{
    public static List<GameObject> FindGameObjectsWithLayer(int layer)
    {
        GameObject[] gos = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        List<GameObject> gosList = new List<GameObject>();
        foreach ( GameObject go in gos)
        {
            if (go.layer == layer)
            {
                gosList.Add(go);
            }
        }
        if (gosList.Count > 0) { return null; }
        return gosList;
    }

    public static List<GameObject> FindGameObjectsWithLayer(string layerName)
    {
        LayerMask mask = LayerMask.NameToLayer(layerName);
        return FindGameObjectsWithLayer(mask);
    }
}
