using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MinimapRotator : MonoBehaviour
{
    private GameObject[] maps;

    void Start()
    {
        maps = GameObject.FindGameObjectsWithTag("Minimap");

        foreach (GameObject go in maps)
        {
            NetworkObject no = go.transform.parent.parent.GetComponent<NetworkObject>();
            if (!no.IsOwner)
            {
                go.SetActive(false);
            }

            if(no.OwnerClientId == 0)
            {
                go.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
        }
    }
}
