using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShopManager : MonoBehaviour
{
    public enum shopOwners
    {
        NONE,
        AMALGAM,
        CHAMPION        
    }
    public shopOwners shopOwner;

    [SerializeField] List<GameObject> amalgamObjs;
    [SerializeField] List<GameObject> championObjs;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void Update()
    {
        if (shopOwner == shopOwners.AMALGAM)
        {
            foreach (GameObject building in amalgamObjs)
            {
                building.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject building in amalgamObjs)
            {
                building.SetActive(false);
            }
        }

        if (shopOwner == shopOwners.CHAMPION)
        {
            foreach (GameObject element in championObjs)
            {
                element.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject element in championObjs)
            {
                element.SetActive(false);
            }
        }
    }
}
