using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

/// <summary>
/// This class manages hitboxes for abilities, usually projections. But should be modular enough to handle any hitbox type.
/// </summary>
public class HitboxManager : MonoBehaviour
{
    BoxCollider boxCollider;
    SphereCollider sphereCollider;
    public void Init(HitboxStats _hitboxStats)
    {
        if (_hitboxStats == null)
        {
            Debug.LogError($"{nameof(_hitboxStats)} is null. Cannot initialize {GetType().Name} in gameobject - {gameObject.name}!.");
            return;
        }

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
