using Unity.Netcode;
using UnityEngine;

public class FUCK : MonoBehaviour
{
    Health health;
    private void Awake()
    {
        health = GetComponent<Health>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            BulletProjectile bulletProjectile = other.GetComponent<BulletProjectile>();
            bulletProjectile.StartDespawn();

            Invoke(nameof(killit), 0.2f);
        }
    }

    private void killit()
    {
        
        health.Damage(1);
    }
}
