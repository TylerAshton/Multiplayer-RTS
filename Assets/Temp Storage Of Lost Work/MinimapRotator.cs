using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MinimapRotator : MonoBehaviour
{
    private GameObject[] maps;
    NetworkObject networkObject;

    void Start()
    {
        maps = GameObject.FindGameObjectsWithTag("Minimap");

        Debug.Log(maps.Length);
        

        foreach (GameObject go in maps)
        {
            Debug.Log(NetworkManager.Singleton.LocalClientId);

            NetworkObject no = go.transform.parent.parent.GetComponent<NetworkObject>();

            if(NetworkManager.Singleton.LocalClientId == 0)
            {
                go.transform.rotation = Quaternion.Euler(0, 0, 180);
                GetComponent<MinimapHandler>().rotateCampfire(new(0,-90,0));
            }
        }
    }

    private NetworkObject FindNetworkObjectInParents(GameObject start)
    {
        networkObject = null;
        if (!TryGetComponent<NetworkObject>(out networkObject))
        {
            FindNetworkObjectInParents(start.transform.parent.gameObject);
        }
        return networkObject;
    }
}
