using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class Health : NetworkBehaviour
{
    [SerializeField] private float hitPoints;
    public float HitPoints => hitPoints;
    [SerializeField] private bool isImmune = false;
    private float maxHealth;
    public float MaxHealth => maxHealth;
    [SerializeField] private float corpseLingerTime = 0;
    [SerializeField] private bool test;
    [SerializeField] private bool isDying = false; 
    public bool IsDying => isDying;
    [SerializeField] private GameObject overlayHealthBar;
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 0, 0);
    [SerializeField] private bool showHealthBar = true;
    [SerializeField] private bool showOnOwnerScreen = false;

    [SerializeField] private GameObject deathVfx;
    [SerializeField] private float deathVfxScale = 1;

    GameObject healthBar;
    private Slider healthSlider;
    private StatManager statManager;
    public event Action OnDeath; 
    public event Action OnRevive; 
    public event Action OnHit;

    [ContextMenu("KILLME")]
    void KILLME()
    {
        Damage(hitPoints);
    }

    private void Awake()
    {
        if (!TryGetComponent<StatManager>(out statManager))
        {
            Debug.LogError($"{GetType().Name} requires {nameof(StatManager)} within gameobject: {gameObject.name}!");
            return;
        }

        if (corpseLingerTime < 0)
        {
            Debug.LogError($"{nameof(corpseLingerTime)} cannot be a null or negative value.");
            return;
        }

        if (hitPoints <= 0)
        {
            Debug.LogError("Invalid health given");
            return;
        }

        if (showHealthBar && healthBarOffset == null)
        {
            Debug.LogError("HealthBar prefab was not given");
            return;
        }
    }

    private void Start()
    {
        maxHealth = hitPoints;
        ShowHealthBar();
    }

    private void Update()
    {
        if (IsServer)
        {
            ApplyRegeneration();
        }
    }

    /// <summary>
    /// Heals the unit by the amount specified by the health regeneration stat, if applicable.
    /// </summary>
    private void ApplyRegeneration()
    {
        if (!IsServer)
        {
            Debug.LogError($"Regeneration can only be applied on the server : {gameObject.name}!");
            return;
        }

        if (isDying)
        {
            return;
        }

        if (!statManager.CurrentStats.TryGetValue(StatType.HealthRegeneration, out float healthRegeneration))
        {
            Debug.LogError($"{nameof(StatType.HealthRegeneration)} not found on {gameObject.name}!");
            return;
        }

        if (healthRegeneration == 0)
        {
            return;
        }

        if (healthRegeneration < 0)
        {
            Debug.LogError($"Health regeneration cannot be negative: {healthRegeneration} on {gameObject.name}");
            return;
        }

        Heal(healthRegeneration * Time.deltaTime);
    }

    private void ShowHealthBar()
    {
        if (!showHealthBar)
        {
            return;
        }

        if (showOnOwnerScreen && IsOwner)
        {
            ShowOverlayHealthBar();
        }
        else
        {
            ShowHoverBar();
        }
    }

    /// <summary>
    /// Used to create your standard healthbar hovering around the character
    /// </summary>
    private void ShowHoverBar()
    {
        healthBar = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
        healthBar.transform.position += healthBarOffset;
        healthSlider = healthBar.GetComponentInChildren<Slider>();

        healthSlider.maxValue = maxHealth;
        healthSlider.value = hitPoints;

        UIStraightener straightener;
        if (!healthBar.TryGetComponent<UIStraightener>(out straightener))
        {
            Debug.LogError($"{nameof(UIStraightener)} not found on {healthBar.name}!");
            return;
        }

        straightener.Init(transform);
    }

    /// <summary>
    /// Shows the health bar on the screen as a HUD, used for champion player's healthbar for the owner only
    /// </summary>
    private void ShowOverlayHealthBar()
    {
        healthBar = Instantiate(overlayHealthBar);
        healthSlider = healthBar.GetComponentInChildren<Slider>();

        healthSlider.maxValue = maxHealth;
        healthSlider.value = hitPoints;
    }

    /// <summary>
    /// Applies damage to the unit, destroying it if health reaches 0 via DestroyObject
    /// </summary>
    /// <param name="_damage"></param>
    public void Damage(float _damage)
    {
        if (!IsServer)
        {
            Debug.LogError("Damage can only be applied on the server");
            return;
        }

        if (isImmune)
        {
            return;
        }

        hitPoints -= _damage;

        if (showHealthBar)
        {
            UpdateHealthBarClientRpc(hitPoints);
        }

        OnHit?.Invoke();

        if (hitPoints <= 0)
        {
            DestroyObject();
        }
    }

    [ClientRpc]
    private void UpdateHealthBarClientRpc(float _currentHealth)
    {
        if (!showHealthBar)
        {
            return;
        }

        if (healthSlider == null)
        {
            Debug.LogError($"{nameof(healthSlider)} is null in {GetType()} within gameobject {gameObject.name}! " +
                $"This might be caused by someone setting HealthRegen to a value before the start function is called.");
            return;
        }
        healthSlider.value = _currentHealth;
    }

    /// <summary>
    /// Increases the current health by the parsed amount within the confines of maxHealth
    /// </summary>
    /// <param name="_health"></param>
    public void Heal(float _health)
    {
        if (!IsServer)
        {
            Debug.LogError("Healing can only be applied on the server!");
            return;
        }
        hitPoints += _health;
        hitPoints = Mathf.Clamp(hitPoints, 0, maxHealth);

        //healthSlider.value = hitPoints;

        if (showHealthBar)
        {
            UpdateHealthBarClientRpc(hitPoints);
        }

    }

    /// <summary>
    /// Destroys the object this is attached to, marking it as dying and running 
    /// any IDestructable logic if applicable before the object
    /// is destroyed. This is the best way to destroy objects.
    /// </summary>
    public void DestroyObject()
    {
        if (!IsServer)
        {
            Debug.LogError("DestroyObject can only be called by the server!");
            return;
        }

        if (isDying == true)
        {
            return;
        }

        isDying = true;     

        OnDeath?.Invoke();

        if (TryGetComponent<Collider2D>(out Collider2D collider))
        {
            collider.enabled = false;
        }

        // Destructable handling

        IDestructible[] destructibles = GetComponents<IDestructible>();

        if (destructibles.Length > 1) // I really don't think we should ever have more than 1 destructible on a single object
        {
            Debug.LogError($"Multiple destructibles found on {gameObject.name}!" +
                $"Please ensure only one destructible is present on each object that implements IDestructible.");
            return;
        }

        destructibles[0].DestroyObject();

        // Revive destruction blocker

        if (destructibles[0] is IRevivable revivable)
        {
            return;
        }

        DestroyHealthRpc();
        Invoke(nameof(Die), corpseLingerTime);
    }

    [Rpc(SendTo.Everyone)]
    private void DestroyHealthRpc()
    {
        if (healthBar == null)
        {
            return;
        }

        Destroy(healthBar);
    }

    /// <summary>
    /// Revives the object its attached to. Marking it as alive, restoring its health to maxHealth
    /// and enabling the collider if applicable.
    /// </summary>
    public void ReviveObject()
    {
        if (!IsServer)
        {
            Debug.LogError("ReviveObject can only be called by the server!");
            return;
        }

        if (isDying == false)
        {
            Debug.LogError("Cannot revive an object that is not dying!");
            return;
        }

        isDying = false;

        OnRevive?.Invoke();

        Heal(maxHealth + Math.Abs(hitPoints));

        if (TryGetComponent<Collider2D>(out Collider2D collider))
        {
            collider.enabled = true;
        }

        IRevivable[] revivables = GetComponents<IRevivable>();

        if (revivables.Length > 1) // I really don't think we should ever have more than 1 destructible on a single object
        {
            Debug.LogError($"Multiple {nameof(IRevivable)} found on {gameObject.name}!" +
                $"Please ensure only one {nameof(IRevivable)} is present on each object that implements {nameof(IRevivable)}.");
            return;
        }

        revivables[0].ReviveObject();
    }

    /// <summary>
    /// Forcably destroys the gameObject, bypassing all DestroyLogic
    /// </summary>
    private void Die()
    {
        if (!IsServer)
        {
            Debug.LogError("Objects can only be destroyed by the server");
            return;
        }
        if (TryGetComponent<NetworkObject>(out var _networkObject))
        {
            if (_networkObject.IsSpawned == false)
            {
                Debug.LogError("FUKED");
            }

            if ((bool)_networkObject.IsSceneObject)
            {
                DestroySceneObjectRpc();
                return;
            }

            _networkObject.Despawn();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void DestroySceneObjectRpc()
    {
        Destroy(gameObject);
    }
}
