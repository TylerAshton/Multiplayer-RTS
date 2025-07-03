using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MinimapHandler : MonoBehaviour
{
    private List<GameObject> units = new List<GameObject>();
    private Dictionary<GameObject, GameObject> UnitToIcon = new Dictionary<GameObject, GameObject>();
    [SerializeField] List<GameObject> icons;

    private void Start()
    {
        updateList();
    }

    private void updateList()
    {
        deleteIcons();
        units.Clear();
        units = LayerList.FindGameObjectsWithLayer("Unit");
        createIcon();
    }

    private void createIcon()
    {
        foreach (GameObject unit in units)
        {
            if (unit.CompareTag("Champion"))
            {
                if (unit.GetComponent<NetworkObject>().IsOwner)
                {
                    GameObject icon = (GameObject)Instantiate(Resources.Load("Icons/Main Champion Icon"), unit.transform);
                    icons.Add(icon);
                    UnitToIcon.Add(unit, icon);
                }
                else
                {
                    GameObject icon = (GameObject)Instantiate(Resources.Load("Icons/Sub Champion Icon"), unit.transform);
                    icons.Add(icon);
                    UnitToIcon.Add(unit, icon);
                }
            }
            else if (unit.CompareTag("Amalgam"))
            {
                GameObject icon = (GameObject)Instantiate(Resources.Load("Icons/Main Champion Icon"), unit.transform, true, );
                icons.Add(icon);
                UnitToIcon.Add(unit, icon);
            }
            else
            {
                Debug.LogError($"UNIT WITHOUT CORRECT TAG : {unit.name}");
            }
        }
    }

    private void deleteIcons()
    {
        foreach(GameObject unit in units)
        {
            try
            {
                Destroy(UnitToIcon[unit]);
            }
            catch (NullReferenceException)
            {
                Debug.LogError("Attempted to kill gameobject that doesnt exist :l");
            }
        }
    }

    private void moveIcons()
    {
        foreach (GameObject unit in units)
        {
            
        }
    }
}
