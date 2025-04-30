using UnityEngine;

public class ChampionAbilityManager : AbilityManager
{
    [SerializeField] private GameObject AbilityUIPrefab;
    private ChampionUIManager UIAbilityManager;

    protected override void Awake()
    {
        base.Awake();

        // Spawn and setup Ability UI
        GameObject AbilityUI = Instantiate(AbilityUIPrefab);
        UIAbilityManager = AbilityUI.GetComponentInChildren<ChampionUIManager>();

        
    }

    protected void Start()
    {
        UIAbilityManager.UpdateGridWithChampionAbilities(this);
    }
}
