using UnityEngine;

public class ConstructionStats : BaseAbilityStat
{
    [SerializeField] private float constructionTime = 5f;
    public float ConstructionTime => constructionTime;

    [SerializeField] private GameObject consutrctablePrefab;
    public GameObject ConstructablePrefab => consutrctablePrefab;


    [SerializeField] private GameObject spawnVFX;
    public GameObject SpawnVFX => spawnVFX;
    [SerializeField] private float spawnVFXScale = 1f;
    public float SpawnVFXScale => spawnVFXScale;

    [SerializeField] private GameObject summonVFX;
    public GameObject SummonVFX => summonVFX;
    [SerializeField] private float summonVFXScale = 1f;
    public float SummonVFXScale => summonVFXScale;
    public float VfxDespawnTime => 5f;
    [SerializeField] private float maxDispersion = 5f;
    public float MaxDispersion => maxDispersion;
    [SerializeField] private float minDisperstion = 5f;
    public float MinDisperstion => minDisperstion;
    [SerializeField] private Vector3 offset = Vector3.zero;

    [SerializeField] private Sprite queueIcon;
    public Sprite QueueIcon => queueIcon;

    public Vector3 Offset => offset;
}
