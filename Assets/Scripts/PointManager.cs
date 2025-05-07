using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PointManager : MonoBehaviour
{
    public static PointManager Instance;
    private Dictionary<ulong, int> playerPoints = new Dictionary<ulong, int>();
    private GameObject[] pointAwarders;
    private List<GameObject> capturePoints = new List<GameObject>();

    [SerializeField] private List<int> DEBUGplayerPoints;

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

            DEBUGplayerPoints.Clear();
            foreach (KeyValuePair<ulong, int> kvp in playerPoints)
            {
                DEBUGplayerPoints.Add(kvp.Value);
            }
        }
    }
    
    IEnumerator generatePoints()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            foreach (GameObject point in capturePoints)
            {
                if (point.GetComponent<CapturePoint>().owner == CapturePoint.owners.AMALGAM)
                {
                    AddPoints(0, 100);
                }
                else if (point.GetComponent<CapturePoint>().owner == CapturePoint.owners.CHAMPION)
                {
                    AddPoints(1, 100);
                    AddPoints(2, 100);
                }
            }
            Debug.Log($"{playerPoints[0]},{playerPoints[1]},{playerPoints[2]}");
            yield return new WaitForSeconds(1f);
            StartCoroutine(generatePoints());
        }
    }

    public int GetPoints(ulong id)
    {
        return playerPoints[id];
    }

    public void AddPoints(ulong id, int points)
    {
        AddPointsToPlayerRpc(id, points);
    }
    
    public void RemovePoints(ulong id, int points)
    {
        RemovePointsFromPlayerRpc(id, points);
    }

    [Rpc(SendTo.Everyone)]
    public void AddPointsToPlayerRpc(ulong id, int points)
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

    [Rpc(SendTo.Everyone)]
    public void RemovePointsFromPlayerRpc(ulong id, int points)
    {
        try
        {
            playerPoints.Add(id, playerPoints[id] - points);
        }
        catch (ArgumentException)
        {
            int temp = playerPoints[id];
            playerPoints.Remove(id);
            playerPoints.Add(id, temp - points);
        }
    }
}
