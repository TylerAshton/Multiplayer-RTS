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
    private AnimationTriggerManager animTriggerManager;
    public AnimationTriggerManager AnimTriggerManager => animTriggerManager;
    private EffectManager effectManager;
    public EffectManager EffectManager => effectManager;
    public Health ChampionHealth => health;
    private Health health;
    public Health Health => health;
    public Transform Transform => throw new System.NotImplementedException();
    public IReadOnlyDictionary<AbilityPosition, Transform> AbilityPositions => throw new System.NotImplementedException();
    public AbilityManager AbilityManager => throw new System.NotImplementedException();
    public Vector3 AimPoint => throw new System.NotImplementedException();
    public IFaction IFaction => throw new System.NotImplementedException();
    public ulong OwnerID => throw new System.NotImplementedException();
    public Faction Faction { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public ChampionAbilityManager ChampionAbilityManager => throw new System.NotImplementedException();
    public Health ChampionHealth => throw new System.NotImplementedException();
    public int Points => throw new System.NotImplementedException();
    public ShopPurchaseManager ShopPurchaseManager => throw new System.NotImplementedException();
    public ulong PlayerID => throw new System.NotImplementedException();

    public void ClearTarget()
    {
        throw new System.NotImplementedException();
    }

    public void DestroyObject()
    {
        throw new System.NotImplementedException();
    }

    public void Lunge(float distance, Vector3 direction, float lungeDuration)
    {
        throw new System.NotImplementedException();
    }

    public void ReviveObject()
    {
        throw new System.NotImplementedException();
    }

    public void SetTarget(Collider castTarget)
    {
        throw new System.NotImplementedException();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
