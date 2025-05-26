using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class AOECone : NetworkBehaviour, IFaction
{
    [SerializeField] float lifeTimeSec = 2f;
    [SerializeField] float angleDegrees = 90f;
    [SerializeField] float range = 4f;
    [SerializeField] float damage = 1f;
    [SerializeField] float armTimeSec = 0.2f;
    [SerializeField] float burnTimeSec = 2f;
    private float age = 0f;

    private Faction faction = Faction.None;
    Faction IFaction.Faction { get => faction; set => faction = value; }

    private void Start()
    {
        if (!IsServer)
        {
            return;
        }

        StartCoroutine(deathTime());
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        age += Time.deltaTime;

        if (age > armTimeSec && age < burnTimeSec)
        {
            ApplyDamage();
        }
    }

    private IEnumerator deathTime()
    {
        yield return new WaitForSeconds(lifeTimeSec);
        GetComponent<NetworkObject>().Despawn();

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
            // Skip if the hit object is part of the same faction
            if (hit.TryGetComponent<IFaction>(out IFaction faction))
            {
                if (faction.Faction == this.faction)
                {
                    continue; 
                }
            }

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