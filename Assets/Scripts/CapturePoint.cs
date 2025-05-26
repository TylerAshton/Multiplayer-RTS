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
        CHAMPION,
        CONTESTED
    }

    private Material[] materials;

    [SerializeField] GameObject circle;
    [SerializeField] ParticleSystem bonfire;
    [SerializeField] GameObject bonfireObj;
    [SerializeField] ShopManager shop;

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
            if (unit.collider.transform.tag == "Champion")
            {
                champs++;
            }
            else if(unit.collider.transform.tag == "Amalgam")
            {
                amalgs++;
            }
        }
        if (champs >= minChamps && amalgs == 0)
        {
            owner = owners.CHAMPION;
        }
        else if (amalgs >= minAmalgs && champs == 0)
        {
            owner = owners.AMALGAM;
        }
        else if (champs > 0 && amalgs > 0)
        {
            owner = owners.CONTESTED;
        }

        if (owner == owners.AMALGAM)
        {
            bonfire.enableEmission = true;
            bonfire.startColor = Color.red;
            circle.GetComponent<MeshRenderer>().material.color = Color.red;
            shop.shopOwner = ShopManager.shopOwners.AMALGAM;
        }
        else if (owner == owners.CHAMPION)
        {
            bonfire.enableEmission = true;
            bonfire.startColor = Color.blue;
            circle.GetComponent<MeshRenderer>().material.color = Color.blue;
            shop.shopOwner = ShopManager.shopOwners.CHAMPION;
        }
        else if (owner == owners.CONTESTED)
        {
            bonfire.enableEmission = true;
            bonfire.startColor = Color.green;
            circle.GetComponent<MeshRenderer>().material.color = Color.green;
            shop.shopOwner = ShopManager.shopOwners.NONE;
        }
        else
        {
            bonfire.enableEmission = false;
            circle.GetComponent<MeshRenderer>().material.color = Color.grey;
            shop.shopOwner = ShopManager.shopOwners.NONE;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position + offset, r);
    }
}
