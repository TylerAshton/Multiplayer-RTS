using System.Collections;
using UnityEngine;

public class AOECone : MonoBehaviour
{
    [SerializeField] float lifeTimeSec = 2f;

    private void Start()
    {
        StartCoroutine(deathTime());
    }

    private IEnumerator deathTime()
    {
        yield return new WaitForSeconds(lifeTimeSec);
        Destroy(gameObject);
    }
}
