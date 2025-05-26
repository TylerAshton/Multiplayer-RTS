using Unity.Netcode;
using UnityEngine;

public class LifeTime : NetworkBehaviour
{
    [SerializeField] private float lifeTime = 0f;
    private float destroyAtTime = Mathf.Infinity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!IsServer) return;

        if (lifeTime <= 0f)
        {
            Debug.LogError("Lifetime cannot be zero or negative!");
            return;
        }

        destroyAtTime = Time.fixedTime + lifeTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;

        // Lifetimer Check
        if (destroyAtTime < Time.fixedTime)
        {
            gameObject.GetComponent<NetworkObject>().Despawn();
            return;
        }
    }
}
