using TMPro;
using Unity.Netcode;
using UnityEngine;

public class RTS_Points : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI points;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        updatePointsUI();
    }

    private void updatePointsUI()
    {
        //Debug.Log(points.text); This was pissing me off, so I commented it out. - H
        points.text = PointManager.Instance.GetPoints(NetworkManager.Singleton.LocalClientId).ToString();
    }
}
