using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [HideInInspector] public Vector3 AimPoint => aimPoint;
    public ulong OwnerID => networkObject.OwnerClientId;
    private Faction faction = Faction.Champion;
    Faction IFaction.Faction { get => faction; set => faction = value; }
    public IFaction IFaction => this;
    public int Points => PointManager.Instance.GetPoints(OwnerID);
    private ShopPurchaseManager shopPurchaseManager;
    public ShopPurchaseManager ShopPurchaseManager => shopPurchaseManager;
    public ulong PlayerID => OwnerID;
    public NetCodeAnimationManager NAnimator => nAnimator;
    private NetCodeAnimationManager nAnimator;

    private ShopDisplayManager ShopDisplayManager;

    private CameraSpawner cameraSpawner; //camera spawner instance
    private GameObject playerCamera; // the camera that the player will be seeing the game through

    public bool inShop = false;
    private StatManager statManager;

    [Header("Revive")]
    [SerializeField] private GameObject soulPrefab;
    [SerializeField] private Vector3 soulSpawnOffset = Vector3.zero;

    [Header("Champion Scripts")]
    [SerializeField] public ChampionControls championControls;
    [SerializeField] public ChampionMovement championMovement;

    public void Lunge(float distance, Vector3 direction, float lungeDuration)
    {
        championMovement.Lunge(distance, direction, lungeDuration);
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
        if (!TryGetComponent<AnimationTriggerManager>(out animTriggerManager))
        {
            Debug.LogError($"{nameof(AnimationTriggerManager)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }

        if (!TryGetComponent<CameraSpawner>(out cameraSpawner))
        {
            Debug.LogError($"{nameof(CameraSpawner)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }

        if (!TryGetComponent<NetworkObject>(out networkObject))
        {
            Debug.LogError($"{nameof(NetworkObject)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }

        if (!TryGetComponent<NetCodeAnimationManager>(out nAnimator))
        {
            Debug.LogError($"{nameof(NetCodeAnimationManager)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }
        if (!TryGetComponent<EffectManager>(out effectManager))
        {
            Debug.LogError($"{nameof(EffectManager)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }
        if (!TryGetComponent<ChampionAbilityManager>(out championAbilityManager))
        {
            Debug.LogError($"{nameof(ChampionAbilityManager)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }
        if (!TryGetComponent<AbilityPositionManager>(out abilityPositionManager))
        {
            Debug.LogError($"{nameof(AbilityPositionManager)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }
        if (!TryGetComponent<CharacterController>(out championMovement.characterController))
        {
            Debug.LogError($"{nameof(CharacterController)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }
        if (!TryGetComponent<StatManager>(out championMovement.statManager))
        {
            Debug.LogError($"{nameof(StatManager)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }
        if (!TryGetComponent<Health>(out health))
        {
            Debug.LogError($"{nameof(Health)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }
        if (!TryGetComponent<ShopDisplayManager>(out ShopDisplayManager))
        {
            Debug.LogError($"{nameof(ShopDisplayManager)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }
        if (!TryGetComponent<ShopPurchaseManager>(out shopPurchaseManager))
        {
            Debug.LogError($"{nameof(ShopPurchaseManager)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }

        if (networkObject.IsOwner)
        {
            cameraSpawner.Init();
            playerCamera = cameraSpawner.SpawnedCamera.transform.gameObject;
        }
    }

    public void AttemptToggleUI() // TODO: This should really be reworked into ShopDisplayManager
    {
        if (inShop && networkObject.IsOwner) // TODO: Move this to ShopDisplayManager?
        {
            ToggleUI();
        }
    }

    public void CloseShopUI()
    {
        /*Debug.Log("Closing Shop");
        Shop playerShop = gameObject.GetComponentInChildren<Shop>(true);
        playerShop.enabled = false;
        foreach (RectTransform child in playerShop.GetComponentInChildren<RectTransform>(true))
        {
            child.gameObject.SetActive(false);
        }*/
        ShopDisplayManager.CloseShopUI();
    }

    public void ToggleUI()
    {
        /*        Shop playerShop = gameObject.GetComponentInChildren<Shop>(true);
                playerShop.enabled = !playerShop.enabled;
                foreach (RectTransform child in playerShop.GetComponentInChildren<RectTransform>(true))
                {
                    child.gameObject.SetActive(!child.gameObject.activeInHierarchy);
                }*/
        ShopDisplayManager.ToggleShopUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
