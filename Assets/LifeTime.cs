using UnityEngine;

public class LifeTime : MonoBehaviour
{
    [SerializeField] private float lifeTime = 0f;
    private float destroyAtTime = Mathf.Infinity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
        // Lifetimer Check
        if (destroyAtTime < Time.fixedTime)
        {
            Destroy(gameObject);
            return;
        }
    }
}
