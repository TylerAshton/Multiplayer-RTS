using UnityEngine;

public class ChampionAbilityManager : AbilityManager
{
    [SerializeField] private GameObject AbilityUIPrefab;
    private AbilityUIManager UIAbilityManager;

    protected override void Awake()
    {
        base.Awake();

        
    }

    protected void Start()
    {
        if (!IsOwner)
        {
            return;
        }

        SpawnUI();
        UIAbilityManager.UpdateGridWithAbilityManager(this);
    }

    /// <summary>
    /// Instantiates and spawns the AbilityUI for the owning player and saves it as the variable.
    /// </summary>
    private void SpawnUI()
    {
        if (!IsOwner)
        {
            Debug.LogError("Attempted to spawn another player's UI");
            return;
        }

        GameObject AbilityUI = Instantiate(AbilityUIPrefab);
        UIAbilityManager = AbilityUI.GetComponentInChildren<AbilityUIManager>();
    }
}
