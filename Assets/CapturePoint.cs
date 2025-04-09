using Unity.Netcode;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;

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
    [SerializeField] ParticleSystem bonfire;
    [SerializeField] GameObject bonfireObj;

    public owners owner = owners.NEUTRAL;

    private void Awake()
    {
        circle.transform.localScale = new Vector3(r, 1, r);
        circle.transform.position = this.transform.position + offset;
        bonfireObj.transform.position = this.transform.position + offset;
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
            bonfire.enableEmission = true;
            bonfire.startColor = Color.red;
            circle.GetComponent<MeshRenderer>().material.color = Color.red;
        }
        else if (owner == owners.CHAMPION)
        {
            bonfire.enableEmission = true;
            bonfire.startColor = Color.blue;
            circle.GetComponent<MeshRenderer>().material.color = Color.blue;
        }
        else
        {
            bonfire.enableEmission = false;
            circle.GetComponent<MeshRenderer>().material.color = Color.grey;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position + offset, r);
    }
}
