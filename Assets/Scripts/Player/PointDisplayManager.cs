using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PointDisplayManager : NetworkBehaviour
{
    [Header("Managers")]
    [SerializeField] private ChampionManager championManager;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI pointsUI;

    private int points => championManager.Points;

    void Update()
    {
        if (!IsOwner) { return; }
        pointsUI.text = points.ToString();
    }
}
