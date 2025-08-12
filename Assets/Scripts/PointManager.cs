using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PointManager : NetworkBehaviour
{
    public static PointManager Instance;
    private Dictionary<ulong, int> playerPoints = new Dictionary<ulong, int>();
    private GameObject[] pointAwarders;
    private List<GameObject> capturePoints = new List<GameObject>();

    [SerializeField] private List<int> DEBUGplayerPoints;

    [SerializeField] private int ChampMaxPoints = 10000;
    [SerializeField] private int AmalgMaxPoints = 15000;

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
                    AddPoints(0, 300);
                }
                else if (point.GetComponent<CapturePoint>().owner == CapturePoint.owners.CHAMPION)
                {
                    AddPoints(1, 300);
                    AddPoints(2, 300);
                }
            }
            yield return new WaitForSeconds(3f);
            StartCoroutine(generatePoints());
        }
    }

    public int GetPoints(ulong id)
    {
        return playerPoints[id];
    }

    public void AddPoints(ulong id, int points)
    {
        if(id == 0)
        {
            AddPointsToPlayerRpc(id, Mathf.Clamp(GetPoints(id) + points, 0, AmalgMaxPoints));
        }
        else
        {
            AddPointsToPlayerRpc(id, Mathf.Clamp(GetPoints(id) + points, 0, ChampMaxPoints));
        }
    }

    public void RemovePoints(ulong id, int points)
    {
        RemovePointsFromPlayerRpc(id, points);
    }

    [Rpc(SendTo.Everyone)]
    private void AddPointsToPlayerRpc(ulong id, int points)
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
    private void RemovePointsFromPlayerRpc(ulong id, int points)
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
