using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class BulletProjectile : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] float speed = 10f;
    [SerializeField] private float damage = 1f;
    [SerializeField] string friendlyTag;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (friendlyTag == "")
        {
            Debug.LogError("Tag isn't assigned");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void Fire()
    {
        rb.linearVelocity = transform.forward * speed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(friendlyTag)) // Friendly fire will not be tolerated
        {
            return;
        }
        if (other.TryGetComponent<Health>(out var _health))
        {
            _health.Damage(damage);
        }
        
        Destroy(gameObject);
    }
}
