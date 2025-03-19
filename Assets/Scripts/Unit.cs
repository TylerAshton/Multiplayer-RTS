using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Unit component is an base class used for all forms of RTS units across the game.
/// Contains logic for all common behaviours such as selection, death, and instructions.
/// </summary>
public class Unit : MonoBehaviour
{
    [SerializeField] GameObject selectionIndiator;
    RTSPlayer rts_Player;

    private void Awake()
    {
        if (selectionIndiator == null)
        {
            Debug.LogError("Unit selection indicator is missing");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        rts_Player = RTSPlayer.instance;


        rts_Player.UnitManager.AddUnit(this); // TODO remove from list on death
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    public virtual void ShowSelectionIndicator()
    {
        selectionIndiator.SetActive(true);
    }

    public virtual void HideSelectionIndicator()
    {
        selectionIndiator.SetActive(false);
    }
}
