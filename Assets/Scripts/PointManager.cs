using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PointManager : MonoBehaviour
{
    public static PointManager Instance;
    public Dictionary<ulong, int> playerPoints = new Dictionary<ulong, int>();
    private GameObject[] pointAwarders;
    private List<GameObject> capturePoints = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        Debug.Log($"THIS IS YOUR ID : { NetworkManager.Singleton.LocalClientId}");
        playerPoints[0] = 0;
        playerPoints[1] = 0;
        playerPoints[2] = 0;
        if (NetworkManager.Singleton.IsServer)
        {
            StartCoroutine(generatePoints());
        }
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            capturePoints.Clear();
            pointAwarders = GameObject.FindGameObjectsWithTag("PointAwarder");
            foreach (GameObject awarder in pointAwarders)
            {
                if (awarder.layer == LayerMask.NameToLayer("Capture"))
                {
                    capturePoints.Add(awarder);
                }
            }
        }
    }
    
    IEnumerator generatePoints()
    {
        foreach (GameObject point in capturePoints)
        {
            if (point.GetComponent<CapturePoint>().owner == CapturePoint.owners.AMALGAM)
            {
                AddPointsToPlayer(0, 100);
            }
            else if (point.GetComponent<CapturePoint>().owner == CapturePoint.owners.CHAMPION)
            {
                AddPointsToPlayer(1, 100);
                AddPointsToPlayer(2, 100);
            }
        }
        Debug.Log($"{playerPoints[0]},{playerPoints[1]},{playerPoints[2]}");
        yield return new WaitForSeconds(1f);
        StartCoroutine(generatePoints());
    }

    private void AddPointsToPlayer(ulong id, int points)
    {
        try
        {
            playerPoints.Add(id, playerPoints[id] + points);
        }
        catch (ArgumentException)
        {
            int temp = playerPoints[id];
            playerPoints.Remove(id);
            playerPoints.Add(id, temp + points);
        }
    }
}
