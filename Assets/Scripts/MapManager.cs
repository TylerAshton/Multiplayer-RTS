using UnityEngine;

public class MapManager : MonoBehaviour
{
    private static MapManager instance;
    public static MapManager Instance => instance;

    [SerializeField] private Collider mapBoundsCollider;
    private static Bounds mapBounds;
    public static Bounds MapBounds => mapBounds;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError($"Can't have multiple {nameof(MapManager)}s. {this.name} | {instance.gameObject.name}!");
            return;
        }

        instance = this;

        SetMapBounds();
    }

        

    private void SetMapBounds()
    {
        if (mapBoundsCollider == null)
        {
            Debug.LogError($"{nameof(mapBoundsCollider)} is null!");
            return;
        }

        mapBounds = mapBoundsCollider.bounds;
    }
}
