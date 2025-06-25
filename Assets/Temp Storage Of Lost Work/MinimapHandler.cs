using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class MinimapHandler : MonoBehaviour
{
    private List<GameObject> units = new List<GameObject>();
    [SerializeField] List<GameObject> icons;

    private void Start()
    {
        updateList();
    }

    private void updateList()
    {
        units.Clear();
        units = LayerList.FindGameObjectsWithLayer("Unit");
    }

    private void moveIcons()
    {
        updateList();
        foreach (GameObject unit in units)
        {
            
        }
    }
}
