using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class BulletProjectile : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] float speed = 10f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb.linearVelocity = transform.forward * speed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}
