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
    [SerializeField] private Animator animator;
    [SerializeField] private float deathAnimationLength = 0;
    [SerializeField] private bool test;

    [SerializeField] private bool isDying = false;
    public bool IsDying => isDying;

    public event Action OnDeath; // Death event, used to begin respawn

    [SerializeField] private GameObject overlayHealthBar;
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 0, 0);
    private Slider healthSlider;
    [SerializeField] private bool showHealthBar = true;
    [SerializeField] private bool showOnOwnerScreen = false;
    private StatManager statManager;

    private void Awake()
    {
        if (!TryGetComponent<Animator>(out animator))
        {
            Debug.LogError("Animator is required for Health");
            return;
        }

        if (!TryGetComponent<StatManager>(out statManager))
        {
            Debug.LogError($"{GetType().Name} requires {nameof(StatManager)} within gameobject: {gameObject.name}!");
            return;
        }

        if (animator != null && deathAnimationLength == 0)
        {
            Debug.LogError("A death animation was set but no length was given");
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
        GameObject healthBar = Instantiate(healthBarPrefab, transform);
        healthBar.transform.position += healthBarOffset;
        healthSlider = healthBar.GetComponentInChildren<Slider>();

        healthSlider.maxValue = maxHealth;
        healthSlider.value = hitPoints;
    }

    /// <summary>
    /// Shows the health bar on the screen as a HUD, used for champion player's healthbar for the owner only
    /// </summary>
    private void ShowOverlayHealthBar()
    {
        GameObject healthBar = Instantiate(overlayHealthBar);
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

        UpdateHealthBarClientRpc(hitPoints);

        animator.SetTrigger("OnHit");

        if (hitPoints <= 0)
        {
            DestroyObject();
        }
    }

    [ClientRpc]
    private void UpdateHealthBarClientRpc(float _currentHealth)
    {
        if (healthSlider == null)
        {
            Debug.LogError($"{nameof(healthSlider)} is null in {GetType()} within gameobject {gameObject.name}! " +
                $"This might be caused by someone setting HealthRegen to a value before the start function is called.");
            return;
        }
        healthSlider.value = _currentHealth; // TODO: Make a setter
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

        healthSlider.value = hitPoints;

        UpdateHealthBarClientRpc(hitPoints);
    }

    /// <summary>
    /// Destroys the object this is attached to, marking it as dying and running 
    /// any animations and IDestructable logic if applicable before the object
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

        if (animator != null)
        {
            animator.Play("Death");
        }


        if (TryGetComponent<Collider2D>(out Collider2D collider))
        {
            collider.enabled = false;
        }

        IDestructible destructible = GetComponent<IDestructible>();

        if (destructible != null)
        {
            destructible.DestroyObject();
        }
        Invoke(nameof(Die), deathAnimationLength);
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

            _networkObject.Despawn();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
