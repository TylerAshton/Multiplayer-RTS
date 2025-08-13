using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class MinimapHandler : MonoBehaviour
{
    public static MinimapHandler Instance;

    private List<GameObject> Rawunits = new List<GameObject>();
    private List<GameObject> units = new List<GameObject>();
    private Dictionary<GameObject, GameObject> UnitToIcon = new Dictionary<GameObject, GameObject>();
    [SerializeField] List<GameObject> icons;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        Init();
    }

    private void Init()
    {
        icons = LayerList.FindGameObjectsWithLayer("Icon");
    }

    public void updateList()
    {
        //deleteIcons();
        units.Clear();
        Rawunits = LayerList.FindGameObjectsWithLayer("Unit");
        cleanList();
        //createIcon();

        // Was previously going to clear all icons and replace them similar to frame regeneration.
        // However this is very memory intensive so I decided to move the prexisting icons instead
    }

    void cleanList()
    {
        foreach(GameObject go in Rawunits)
        {
            if (go.CompareTag("Champion") || go.CompareTag("Amalgam"))
            {
                units.Add(go);
            }
        }
    }

    public GameObject changeCampfire(GameObject icon, string owner)
    {
        GameObject Newicon = (GameObject)Instantiate(Resources.Load($"Icons/{owner} Campfire Icon"), icon.transform.position, Quaternion.Euler(0,90,0), icon.transform.parent);
        SetLayer(Newicon);
        icons.Add(Newicon);
        //Debug.Log(icon.GetComponentInParent<Transform>().gameObject.name);
        Destroy(icon);
        icons.Remove(icon);
        return Newicon;
    }

    public void rotateCampfire(Vector3 rotation)
    {
        foreach (GameObject go in icons)
        {
            if (go.name.ToUpper().Contains("CAMPFIRE"))
            {
                go.transform.rotation = Quaternion.Euler(rotation);
            }
        }
    }


    public void createIcon()
    {
        foreach (GameObject unit in units)
        {
            createIcon(unit);
        }
    }

    public void createIcon(GameObject unit)
    {
        if (unit.CompareTag("Champion"))
        {
            if (unit.GetComponent<NetworkObject>().IsOwner)
            {
                GameObject icon = (GameObject)Instantiate(Resources.Load("Icons/Main Champion Icon"), new(unit.transform.position.x, unit.transform.position.y - 190, unit.transform.position.z), Quaternion.identity, unit.transform);
                SetLayer(icon);
                icons.Add(icon);
                UnitToIcon.Add(unit, icon);
            }
            else
            {
                GameObject icon = (GameObject)Instantiate(Resources.Load("Icons/Sub Champion Icon"), new(unit.transform.position.x, unit.transform.position.y - 190, unit.transform.position.z), Quaternion.identity, unit.transform);
                SetLayer(icon);
                icons.Add(icon);
                UnitToIcon.Add(unit, icon);
            }
        }
        else if (unit.CompareTag("Amalgam"))
        {
            GameObject icon = (GameObject)Instantiate(Resources.Load("Icons/Sub Amalgam Icon"), new(unit.transform.position.x, unit.transform.position.y - 190, unit.transform.position.z), Quaternion.identity, unit.transform);
            SetLayer(icon);
            icons.Add(icon);
            UnitToIcon.Add(unit, icon);
            Action handler = () => deleteIcon(unit);
            unit.GetComponent<Health>().OnDeath += handler;
        }
        else
        {
            Debug.LogError($"UNIT WITHOUT CORRECT TAG : {unit.name}");
        }
    }

    private void SetLayer(GameObject go)
    {
        go.layer = LayerMask.NameToLayer("Icon");
    }

    private void deleteAllIcons()
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

    public void deleteIcon(GameObject unit)
    {
        try
        {
            Destroy(UnitToIcon[unit]);
        }
        catch (NullReferenceException)
        {
            Debug.LogError("How the fuck did you fuck that up?");
        }
    }

    private void moveIcons()
    {
        foreach (GameObject unit in units)
        {
            GameObject _icon = UnitToIcon[unit];
            _icon.transform.position = new(unit.transform.position.x, _icon.transform.position.y, unit.transform.position.z);
        }
    }
}
