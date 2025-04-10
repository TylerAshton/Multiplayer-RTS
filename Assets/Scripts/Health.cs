using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class Health : MonoBehaviour
{
    [SerializeField] private float hitPoints;
    public float HitPoints => hitPoints;
    [SerializeField] private Animator animator;
    [SerializeField] private float deathAnimationLength = 0;
    [SerializeField] private bool test;

    [SerializeField] private bool isDying = false;
    public bool IsDying => isDying;

    public event Action OnDeath; // Death event, used to begin respawn

    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 0, 0);
    private Slider healthSlider;
    [SerializeField] private bool showHealthBar = true;
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
        if (showHealthBar)
        {
            GameObject healthBar = Instantiate(healthBarPrefab, transform);
            healthBar.transform.position += healthBarOffset;
            healthSlider = healthBar.GetComponentInChildren<Slider>();

            healthSlider.maxValue = hitPoints;
            healthSlider.value = hitPoints;
        }
    }

    /// <summary>
    /// Applies damage to the unit, destroying it if health reaches 0 via DestroyObject
    /// </summary>
    /// <param name="_damage"></param>
    public void Damage(float _damage)
    {
        hitPoints -= _damage;

        healthSlider.value = hitPoints; // TODO: Make a setter

        animator.SetTrigger("OnHit");

        if (hitPoints <= 0)
        {
            DestroyObject();
        }
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
