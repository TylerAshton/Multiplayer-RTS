using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface IShopUser
{
    ChampionAbilityManager ChampionAbilityManager { get; }
    Health ChampionHealth { get; }
    int Points { get; }
    ShopPurchaseManager ShopPurchaseManager { get; }

    ulong PlayerID { get; }
}

public class ChampionManager : NetworkBehaviour, ICharacterAbilityUser, IFaction, IRevivable, IShopUser
{
    private NetworkObject networkObject; // current networkObject attached to the player
    private AnimationTriggerManager animTriggerManager;
    public AnimationTriggerManager AnimTriggerManager => animTriggerManager;
    private EffectManager effectManager;
    public EffectManager EffectManager => effectManager;
    public Health ChampionHealth => health;
    private Health health;
    public Health Health => health;
    public Transform Transform => transform;
    private AbilityPositionManager abilityPositionManager;
    public IReadOnlyDictionary<AbilityPosition, Transform> AbilityPositions => abilityPositionManager.AbilityPositions;
    private ChampionAbilityManager championAbilityManager;
    public ChampionAbilityManager ChampionAbilityManager => championAbilityManager;
    public AbilityManager AbilityManager => championAbilityManager;
    private Vector3 aimPoint;
    public Vector3 AimPoint => aimPoint;
    public ulong OwnerID => networkObject.OwnerClientId;
    private Faction faction = Faction.Champion;
    Faction IFaction.Faction { get => faction; set => faction = value; }
    public IFaction IFaction => this;
    public int Points => PointManager.Instance.GetPoints(OwnerID);
    private ShopPurchaseManager shopPurchaseManager;
    public ShopPurchaseManager ShopPurchaseManager => shopPurchaseManager;
    public ulong PlayerID => OwnerID;

    [Header("Revive")]
    [SerializeField] private GameObject soulPrefab;
    [SerializeField] private Vector3 soulSpawnOffset = Vector3.zero;

    [Header("Champion Scripts")]
    [SerializeField] public ChampionControls championControls;
    [SerializeField] public ChampionMovement championMovement;

    public void Lunge(float distance, Vector3 direction, float lungeDuration)
    {
        throw new System.NotImplementedException();
    }

    public void ReviveObject()
    {
        championControls.ToggleControlsRpc(true);
    }

    public void DestroyObject()
    {
        championControls.ToggleControlsRpc(false);
        SpawnSoul();
    }

    private void SpawnSoul()
    {
        GameObject soul = Instantiate(soulPrefab, transform.position + soulSpawnOffset, Quaternion.identity);
        soul.GetComponent<NetworkObject>().Spawn();
        soul.GetComponent<ReviveSoul>().Init(gameObject);

    }

    public void SetTarget(Collider castTarget) // TODO: this will be updated in I believe 0.7?? - H
    {
        throw new System.NotImplementedException();
    }

    public void ClearTarget()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// ///////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
