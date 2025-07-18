using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShopManager : MonoBehaviour // TODO: Honestly I'd rename this script as it's not doing anything shopwise
{
    public enum shopOwners
    {
        NONE,
        AMALGAM,
        CHAMPION        
    }
    public shopOwners shopOwner;

    [SerializeField] List<SelectableObject> amalgamObjs;
    [SerializeField] List<GameObject> championObjs;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void Update()
    {
        if (shopOwner == shopOwners.AMALGAM)
        {
            foreach (SelectableObject unit in amalgamObjs)
            {
                if (unit is ConstructionPad constructionPad)
                {
                    constructionPad.territoryOwned = true;
                }

                else
                {
                    unit.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            foreach (SelectableObject unit in amalgamObjs)
            {
                if (unit is ConstructionPad constructionPad)
                {
                    constructionPad.territoryOwned = false;
                }

                else
                {
                    unit.gameObject.SetActive(false);
                }
            }
        }
    }
}
