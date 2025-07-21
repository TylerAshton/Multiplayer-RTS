using UnityEngine;

public class ConstructionStats : BaseAbilityStat
{
    [SerializeField]
    private float constructionTime = 5f;
    public float ConstructionTime => constructionTime;

    [SerializeField] private GameObject consutrctablePrefab;
    public GameObject ConstructablePrefab => consutrctablePrefab;

}
