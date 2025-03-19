using System;
using UnityEngine;

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

    private void Awake()
    {
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
    }

    /// <summary>
    /// Applies damage to the unit, destroying it if health reaches 0
    /// </summary>
    /// <param name="_damage"></param>
    public void Damage(float _damage)
    {
        hitPoints -= _damage;

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

    private void Die()
    {
        Destroy(gameObject);
    }
}
