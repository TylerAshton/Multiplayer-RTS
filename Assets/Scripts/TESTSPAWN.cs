using UnityEngine;

public class TESTSPAWN : MonoBehaviour
{
    [SerializeField] GameObject spawne;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instantiate(spawne, transform.position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        Instantiate(spawne, transform.position, Quaternion.identity);
    }
}
