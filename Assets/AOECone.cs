using System.Collections;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class AOECone : MonoBehaviour
{
    [SerializeField] float lifeTimeSec = 2f;
    [SerializeField] float angleDegrees = 90f;
    [SerializeField] float range = 4f;
    [SerializeField] float damage = 1f;
    [SerializeField] float armTimeSec = 0.2f;
    [SerializeField] float burnTimeSec = 2f;
    private float age = 0f;

    private void Start()
    {
        StartCoroutine(deathTime());
    }

    private void Update()
    {
        age += Time.deltaTime;

        if (age > armTimeSec && age < burnTimeSec)
        {
            ApplyDamage();
        }
    }

    private IEnumerator deathTime()
    {
        yield return new WaitForSeconds(lifeTimeSec);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {

    }

    private void ApplyDamage()
    {
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;
        float cosAngle = Mathf.Cos(angleDegrees * 0.5f * Mathf.Deg2Rad); // Conversion of our angleDegrees to a cos for dot. 
                                                                         // We use half as the full cone is angleDegrees

        Collider[] hits = Physics.OverlapSphere(origin, range);
        foreach (var hit in hits)
        {
            Vector3 toTarget = (hit.transform.position - origin).normalized;
            float dot = Vector3.Dot(forward, toTarget);

            if (dot >= cosAngle) // If is within our cone degrees, hit
            {
                if (hit.TryGetComponent(out Health _health))
                {
                    _health.Damage(damage * Time.deltaTime);
                }
            }
        }
    }
}