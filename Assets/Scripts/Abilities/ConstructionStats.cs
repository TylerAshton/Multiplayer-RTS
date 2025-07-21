using UnityEngine;

public class ConstructionStats : BaseAbilityStat
{
    [SerializeField]
    private float constructionTime = 5f;
    public float ConstructionTime => constructionTime;

    [SerializeField] private GameObject consutrctablePrefab;
    public GameObject ConstructablePrefab => consutrctablePrefab;

    [SerializeField] private GameObject spawnVFX;
    [SerializeField] private float maxDispersion = 5f;
    public float MaxDispersion => maxDispersion;
    [SerializeField] private float minDisperstion = 5f;
    public float MinDisperstion => minDisperstion;
    [SerializeField] private Vector3 offset = Vector3.zero;
    public Vector3 Offset => offset;

}
