using UnityEngine;

public class ChampionAbilityManager : AbilityManager
{
    [SerializeField] private GameObject AbilityUIPrefab;
    private AbilityUIManager UIAbilityManager;

    public AbilityUIManager pubUIAbilityManager => UIAbilityManager;

    protected override void Awake()
    {
        base.Awake();

        // Spawn and setup Ability UI
        GameObject AbilityUI = Instantiate(AbilityUIPrefab);
        UIAbilityManager = AbilityUI.GetComponentInChildren<AbilityUIManager>();
    }

    protected void Start()
    {
        UIAbilityManager.UpdateGridWithAbilityManager(this);
    }

    public void UpdateGrid()
    {
        UIAbilityManager.UpdateGridWithAbilityManager(this);
    }
}
