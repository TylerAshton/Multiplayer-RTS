using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;
public class Health : NetworkBehaviour
{
    [SerializeField] private float hitPoints;
    public float HitPoints => hitPoints;

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
    private void Awake()
    {
        if (!TryGetComponent<Animator>(out animator))
        {
            Debug.LogError("Animator is required for Health");
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
        healthSlider.value = _currentHealth; // TODO: Make a setter
    }

    /// <summary>
    /// Increases the current health by the parsed amount within the confines of maxHealth
    /// </summary>
    /// <param name="_health"></param>
    public void Heal(float _health)
    {
        hitPoints += _health;

        healthSlider.value = hitPoints;

        hitPoints = Mathf.Clamp(hitPoints, 0, maxHealth);
    }

    /// <summary>
    /// Destroys the object this is attached to, marking it as dying and running 
    /// any animations and IDestructable logic if applicable before the object
    /// is destroyed. This is the best way to destroy objects.
    /// </summary>
    public void DestroyObject()
    {
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
        if(TryGetComponent<NetworkObject>(out var _networkObject))
        {
            _networkObject.Despawn();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
