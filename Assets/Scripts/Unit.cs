using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health))]
/// <summary>
/// Unit component is an base class used for all forms of RTS units across the game.
/// Contains logic for all common behaviours such as selection, death, and instructions.
/// </summary>
public class Unit : MonoBehaviour, IDestructible
{
    [SerializeField] GameObject selectionIndiator;
    RTSPlayer rts_Player;
    Health health;

    private void Awake()
    {
        if (selectionIndiator == null)
        {
            Debug.LogError("Unit selection indicator is missing");
        }

        health = GetComponent<Health>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        if (RTSPlayer.instance == null)
        {
            Debug.LogError("RTS Manager doesn't exist");
            return;
        }

        rts_Player = RTSPlayer.instance;


        rts_Player.UnitManager.AddUnit(this);
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

    public virtual void DestroyObject()
    {
        rts_Player.UnitManager.RemoveUnit(this);
    }
}
