using System;
using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.HableCurve;
using static UnityEngine.UI.Image;

/// <summary>
/// This class manages hitboxes for abilities, usually projections. But should be modular enough to handle any hitbox type.
/// </summary>
public class HitboxManager : MonoBehaviour, IFaction
{
    private HitboxStats hitboxStats;
    private BoxCollider boxCollider;
    private SphereCollider sphereCollider;

    public event Action<Collider> OnHitboxTriggerStay;
    public Faction Faction { get => faction; set => faction = value; }
    private Faction faction = Faction.None;
    private void Init(HitboxStats _hitboxStats)
    {
        if (_hitboxStats == null)
        {
            Debug.LogError($"{nameof(_hitboxStats)} is null. Cannot initialize {GetType().Name} in gameobject - {gameObject.name}!.");
            return;
        }

        hitboxStats = _hitboxStats;

        switch (_hitboxStats.HitboxType) // I think type conditionals are fine. Doing derived classes would be overkill for this.
        {
            case HitboxType.Sphere:
                StartCoroutine(ResizeSphere(_hitboxStats.SphereEndRadius, _hitboxStats.SizeChangeTime));
                break;
            case HitboxType.Box:
                StartCoroutine(ResizeSquare(_hitboxStats.BoxForwardExtension, _hitboxStats.BoxWidthExtension, _hitboxStats.SizeChangeTime));
                break;
            case HitboxType.Cone:
                StartCoroutine(ResizeSphere(_hitboxStats.SphereEndRadius, _hitboxStats.SizeChangeTime));
                break;
            default:
                EditorGUILayout.HelpBox("Unknown hitbox type!", MessageType.Error);
                break;
        }
    }

    private void OnDrawGizmos()
    {
        switch (hitboxStats.HitboxType)
        {
            case HitboxType.Sphere:
                DrawSphereCollider();
                break;
            case HitboxType.Box:
                DrawBoxCollider();
                break;
            case HitboxType.Cone:
                DrawConeCollider();
                break;
            default:
                EditorGUILayout.HelpBox("Unknown hitbox type!", MessageType.Error);
                break;
        }
    }

    private void OnTriggerStay(Collider _other) // TODO: Rework the whole thing to use phyisics instead
    {
        if (!_other.TryGetComponent<IFaction>(out IFaction _faction))
        {
            return;
        }

        if (_faction.Faction == faction)
        {
            return;
        }

        // Cone filter, if the collider is not within the cone ignore it
        if (hitboxStats.HitboxType == HitboxType.Cone)
        {
            if (!IsWithinCone(_other)) 
            {
                return;
            }
        }

        OnHitboxTriggerStay?.Invoke(_other);
    }

    /// <summary>
    /// Returns true if the collider is within the cone of the hitbox.
    /// </summary>
    /// <param name="_other"></param>
    /// <returns></returns>
    private bool IsWithinCone(Collider _other)
    {
        Vector3 contactDirection = _other.transform.position - transform.position;
        contactDirection.y = 0;

        Vector3 forwardDirection = transform.forward;
        forwardDirection.y = 0; 

        float dot = Vector3.Dot(forwardDirection.normalized, contactDirection.normalized);
        
        float cosAngle = Mathf.Cos(hitboxStats.ConeAngle * 0.5f * Mathf.Deg2Rad);

        return dot >= cosAngle;
    }

    /// <summary>
    /// Draws a cone gizmo for the cone collider.
    /// </summary>
    private void DrawConeCollider()
    {
        int segments = 10;
        float angle = hitboxStats.ConeAngle;

        Gizmos.color = Color.yellow;

        Vector3 center = transform.TransformPoint(sphereCollider.center);
        Vector3 direction = transform.forward;

        for (int i = 0; i <= segments; i++)
        {
            float frac = (float)i / segments;
            float theta = Mathf.Lerp(-angle / 2f, angle / 2f, frac);
            Quaternion rot = Quaternion.AngleAxis(theta, transform.up);
            Vector3 dir = rot * direction;

            Gizmos.DrawLine(center, center + dir * sphereCollider.radius);
        }

    }

    /// <summary>
    /// Draws a sphere gizmo for the sphere collider.
    /// </summary>
    private void DrawSphereCollider()
    {
        Gizmos.color = Color.yellow;

        Vector3 center = transform.TransformPoint(sphereCollider.center);

        float scaledRadius = sphereCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);

        Gizmos.DrawWireSphere(center, scaledRadius);
    }

    /// <summary>
    /// Draws a box gizmo for the box collider.
    /// </summary>
    private void DrawBoxCollider()
    {
        Gizmos.color = Color.yellow;

        // Rotations Matris BS that alligns the transform and rotation
        Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(boxCollider.center), transform.rotation, Vector3.Scale(transform.lossyScale, Vector3.one));

        Gizmos.DrawWireCube(Vector3.zero, boxCollider.size);

        Gizmos.matrix = Matrix4x4.identity; // Reset Matrix
    }

    /// <summary>
    /// Resizes the sphere collider over a duration to the target radius.
    /// </summary>
    /// <param name="_targetRadius"></param>
    /// <param name="_duration"></param>
    /// <returns></returns>
    private IEnumerator ResizeSphere(float _targetRadius, float _duration)
    {
        float originalRadius = sphereCollider.radius;
        float timeElapsed = 0f;
        while (sphereCollider.radius != _targetRadius)
        {
            float newRadius = Mathf.Lerp(originalRadius, _targetRadius, timeElapsed / _duration);
            sphereCollider.radius = newRadius;
            timeElapsed += Time.deltaTime;

            yield return null;
        }
    }

    /// <summary>
    /// Resizes the box collider over a duration to the target forward and width extensions.
    /// </summary>
    /// <param name="_targetForwardExtension"></param>
    /// <param name="_targetWidthExtension"></param>
    /// <param name="_duration"></param>
    /// <returns></returns>
    private IEnumerator ResizeSquare(float _targetForwardExtension, float _targetWidthExtension, float _duration)
    {
        Vector3 boxColliderOriginalSize = boxCollider.size;
        Vector3 boxColliderOriginalCenter = boxCollider.center;
        float currentForwardExtension = 0;
        float currentWidthExtension = 0;
        float timeElapsed = 0f;

        while (currentForwardExtension != _targetForwardExtension || currentWidthExtension != _targetWidthExtension)
        {
            if (currentForwardExtension != _targetForwardExtension)
            {
                currentForwardExtension = Mathf.Lerp(0, _targetForwardExtension, timeElapsed / _duration);
                boxCollider.size = new Vector3(boxCollider.size.x, boxCollider.size.y, boxColliderOriginalSize.z + currentForwardExtension);
                boxCollider.center = new Vector3(boxCollider.center.x, boxCollider.center.y, boxColliderOriginalCenter.z + (currentForwardExtension / 2));
            }

            if (currentWidthExtension != _targetWidthExtension)
            {
                currentWidthExtension = Mathf.Lerp(0, _targetWidthExtension, timeElapsed / _duration);
                boxCollider.size = new Vector3(boxColliderOriginalSize.x + currentWidthExtension, boxCollider.size.y, boxCollider.size.z);
            }

            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// Applies the projectile stats to the bullet, this is used to set the stats of the bullet when it is instantiated
    /// </summary>
    /// <param name="_projectileStats"></param>
    /// 
    [Rpc(SendTo.Everyone)]
    private void ApplyHitboxStatsRpc(string _hitboxStatsID)
    {
        HitboxStats hitboxStats = AbilityStatsRegistry.GetProjectileStat<HitboxStats>(_hitboxStatsID);

        if (hitboxStats == null)
        {
            Debug.LogError("ProjectileStats is null");
            return;
        }

        if (!hitboxStats.IsValid())
        {
            Debug.LogError("ProjectileStats is not valid, check the console for more information");
            return;
        }

        switch (hitboxStats.HitboxType) // I think type conditionals are fine. Doing derived classes would be overkill for this.
        {
            case HitboxType.Sphere:
                SpawnSphere(hitboxStats);
                break;
            case HitboxType.Box:
                SpawnBox(hitboxStats);
                break;
            case HitboxType.Cone:
                SpawnCone(hitboxStats);
                break;
            default:
                EditorGUILayout.HelpBox("Unknown hitbox type!", MessageType.Error);
                break;
        }

        Init(hitboxStats);
    }

    private void SpawnSphere(HitboxStats _hitboxStats)
    {
        sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.center = _hitboxStats.Offset;
        sphereCollider.radius = _hitboxStats.SphereStartRadius;
    }
    private void SpawnBox(HitboxStats _hitboxStats)
    {
        boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        boxCollider.center = _hitboxStats.Offset;
        boxCollider.size = _hitboxStats.BoxStartSize;
    }
    private void SpawnCone(HitboxStats _hitboxStats)
    {
        SpawnSphere(_hitboxStats);
    }

    public void ApplyHitboxStatsWithID(string _hitboxStatsID)
    {
        ApplyHitboxStatsRpc(_hitboxStatsID);
    }
}
