using Unity.Netcode;
using UnityEngine;

public class CapturePoint : NetworkBehaviour
{
    [SerializeField] float r = 10;
    [SerializeField] Vector3 offset = Vector3.zero;
    [SerializeField] private LayerMask mask;
    public int champs = 0;
    public int amalgs = 0;
    [SerializeField] private int minChamps = 0;
    [SerializeField] private int minAmalgs = 0;

    [SerializeField] public enum owners
    {
        AMALGAM,
        NEUTRAL,
        CHAMPION
    }

    private Material[] materials;

    [SerializeField] GameObject circle;

    public owners owner = owners.NEUTRAL;

    private void Awake()
    {
        circle.transform.localScale = new Vector3(r, 1, r);
        circle.transform.position = this.transform.position + offset;
    }

    void Update()
    {
        champs = 0;
        amalgs = 0;
        RaycastHit[] units = Physics.SphereCastAll(this.transform.position + offset, r, Vector3.forward, 0, mask);
        
        foreach (RaycastHit unit in units)
        {
            Debug.Log("SOMEONE REACHED THIS POINT");
            if (unit.collider.transform.tag == "Champion")
            {
                Debug.Log("CHAMPS REACHED THIS POINT");
                champs++;
            }
            else if(unit.collider.transform.tag == "Amalgam")
            {
                Debug.Log("AMALGS REACHED THIS POINT");
                amalgs++;
            }
        }
        if (champs > minChamps && amalgs == 0)
        {
            owner = owners.CHAMPION;
        }
        else if (amalgs > minAmalgs && champs == 0)
        {
            owner = owners.AMALGAM;
        }

        if (owner == owners.AMALGAM)
        {
            circle.GetComponent<MeshRenderer>().material.color = Color.magenta;
        }
        else if (owner == owners.CHAMPION)
        {
            circle.GetComponent<MeshRenderer>().material.color = Color.blue;
        }
        else
        {
            circle.GetComponent<MeshRenderer>().material.color = Color.gray;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position + offset, r);
    }
}
